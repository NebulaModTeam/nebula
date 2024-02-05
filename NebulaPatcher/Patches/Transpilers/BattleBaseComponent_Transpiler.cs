#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaAPI.GameState;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory.Storage;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(BattleBaseComponent))]
internal class BattleBaseComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BattleBaseComponent.AutoPickTrash))]
    public static IEnumerable<CodeInstruction> BattleBaseComponent_AutoPickTrash_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iLGenerator)
    {
        try
        {
            // Insert the following guard at the begining to only let host execute auto pick trash
            // if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient) return false;
            var codeMatcher = new CodeMatcher(instructions, iLGenerator)
                .MatchForward(false, new CodeMatch(OpCodes.Ldarg_1))
                .CreateLabel(out var label)
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                    new CodeInstruction(OpCodes.Brfalse_S, label),
                    new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.Session))),
                    new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(MultiplayerSession), nameof(MultiplayerSession.LocalPlayer))),
                    new CodeInstruction(OpCodes.Call,
                    AccessTools.DeclaredPropertyGetter(typeof(ILocalPlayer), nameof(ILocalPlayer.IsClient))),
                    new CodeInstruction(OpCodes.Brfalse_S, label),
                    new CodeInstruction(OpCodes.Ret)
                );

            /*Handle sand sharing between players
              From:
                int count = trashObjPool[num4].count;
                if (item == 1099)
                {
                    Player mainPlayer = factory.gameData.mainPlayer;
                    ...
                    break; //jump to the end
                }
             To:
                int count = trashObjPool[num4].count;
                if (item == 1099)
                {
                    BattleBaseComponent_Transpiler.AddPlayerSandCount(trashSystem, num4, count); //Replace the following code to avoid HarmonyX bug
                    return; //ret
                    Player mainPlayer = factory.gameData.mainPlayer;
                    ...
                    break;
                }
            */

            codeMatcher
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TrashObject), nameof(TrashObject.count))),
                    new CodeMatch(OpCodes.Stloc_S));
            var loadTrashId = new CodeInstruction(OpCodes.Ldloc_S, codeMatcher.InstructionAt(-3).operand);
            var loadCount = new CodeInstruction(OpCodes.Ldloc_S, codeMatcher.Operand);
            codeMatcher
                .MatchForward(false, new CodeMatch(OpCodes.Bne_Un))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_2), //TrashSystem trashSystem
                    loadTrashId,
                    loadCount,
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BattleBaseComponent_Transpiler), nameof(AddPlayerSandCount))),
                    new CodeInstruction(OpCodes.Ret)
                );

            // Broadcast pick up item from host to clients
            // From: nextStorage.AddItem(item, num11, num12, out num14, true);
            // To:   BattleBaseComponent_Transpiler.AddItem(nextStorage, item, num11, num12, out num14, true, factory);

            codeMatcher
                .MatchForward(false, new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "AddItemFiltered"))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
                .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(BattleBaseComponent_Transpiler), nameof(AddItemFiltered)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler BattleBaseComponent.AutoPickTrash failed.");
            Log.Error(e);
            return instructions;
        }
    }

    private static void AddPlayerSandCount(TrashSystem trashSystem, int trashId, int sandCount)
    {
        if (Multiplayer.IsActive && !Config.Options.SyncSoil) // Host
        {
            var connectedPlayers = Multiplayer.Session.Server.Players.Connected;
            {
                var totalPlayerCount = connectedPlayers.Count + (Multiplayer.IsDedicated ? 0 : 1);
                if (totalPlayerCount > 0)
                {
                    // Sand gain is split between all connecting players
                    sandCount = (int)((float)sandCount / totalPlayerCount + 0.5f);
                    var packet = new PlayerSandCount(sandCount, true);
                    Multiplayer.Session.Server.SendPacket(packet);
                }
            }
        }

        var mainPlayer = GameMain.data.mainPlayer;
        lock (mainPlayer)
        {
            mainPlayer.SetSandCount(mainPlayer.sandCount + sandCount);
            mainPlayer.NotifySandCollectFromTrash(sandCount);
            trashSystem.RemoveTrash(trashId);
        }
    }

    private static int AddItemFiltered(StorageComponent storage, int itemId, int count, int inc, ref int remainInc, bool useBan, PlanetFactory factory)
    {
        var addedCount = storage.AddItemFiltered(itemId, count, inc, out remainInc, useBan);
        if (!Multiplayer.IsActive)
        {
            return addedCount;
        }
        if (addedCount > 0)
        {
            //Host broadcast the event to clients
            var planetId = factory.planetId;
            var starId = factory.planet.star.id;
            Multiplayer.Session.Network.SendPacketToStar(
                new StorageSyncRealtimeChangePacket(
                storage.id,
                StorageSyncRealtimeChangeEvent.AddItemFiltered,
                itemId, addedCount, useBan, planetId), starId);
        }
        return addedCount;
    }
}
