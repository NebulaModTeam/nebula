#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(ConstructionModuleComponent))]
internal class ConstructionModuleComponent_Transpiler
{
    [HarmonyTranspiler, HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(ConstructionModuleComponent.PlaceItems))]
    public static IEnumerable<CodeInstruction> PlaceItems_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Sync Prebuild.itemRequired changes by player, insert local method call after player.package.TakeTailItems
                After:  player.package.TakeTailItems(ref itemId, ref count, out inc, false);
                Insert: SendPacket(factory, ptr3, count);
                Before: Assert.True(count == ptr3.itemRequired);
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_2),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(Player), nameof(Player.package))),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldloca_S),
                    new CodeMatch(OpCodes.Ldc_I4_0),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "TakeTailItems"),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PrebuildData), nameof(PrebuildData.itemRequired)))
                )
                .Repeat(
                    matcher => matcher
                        .Advance(-2)
                        .InsertAndAdvance(
                            new CodeInstruction(OpCodes.Ldarg_1),
                            new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(1).operand),
                            new CodeInstruction(OpCodes.Ldloc_S, matcher.InstructionAt(-4).operand),
                            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ConstructionModuleComponent_Transpiler), nameof(SendPacket)))
                        )
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler ConstructionModuleComponent.PlaceItems failed.");
            Log.Error(e);
            return instructions;
        }
    }

    private static void SendPacket(PlanetFactory factory, ref PrebuildData prebuild, int itemCount)
    {
        if (!Multiplayer.IsActive) return;

        var packet = new PrebuildItemRequiredUpdate(factory.planetId, prebuild.id, itemCount);
        Multiplayer.Session.Network.SendPacketToLocalStar(packet);
    }
}
