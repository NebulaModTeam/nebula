#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlayerAction_Mine))]
internal class PlayerAction_Mine_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerAction_Mine.GameTick))]
    public static IEnumerable<CodeInstruction> PlayerActionMine_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions)
            .MatchForward(true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldelema),
                new CodeMatch(OpCodes.Ldflda),
                new CodeMatch(OpCodes.Dup),
                new CodeMatch(OpCodes.Ldind_I4),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Sub),
                new CodeMatch(OpCodes.Stind_I4));

        if (!codeMatcher.IsInvalid)
        {
            return codeMatcher
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<FetchVeinMineAmount>(_this =>
                {
                    // do we need to check for the event here? its very unlikely that we call the GameTick() by hand...
                    if (Multiplayer.IsActive && !Multiplayer.Session.Planets.IsIncomingRequest)
                    {
                        Multiplayer.Session.Network.SendPacketToLocalStar(new VegeMinedPacket(_this.player.planetId,
                            _this.miningId,
                            _this.player.factory.veinPool[_this.miningId].amount, true));
                    }

                    return 0;
                }))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .InstructionEnumeration();
        }
        Log.Error("PlayerActionMine_Transpiler failed. Mod version not compatible with game version.");
        return codeInstructions;

    }

    private delegate int FetchVeinMineAmount(PlayerAction_Mine _this);
}
