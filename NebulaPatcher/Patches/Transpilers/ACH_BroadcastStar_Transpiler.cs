#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(ACH_BroadcastStar))]
internal class ACH_BroadcastStar_Transpiler
{
    /*
     * Returns early if
     * instance.gameData.factories[factoryId]?.powerSystem?.genPool[generatorId] == null || instance.gameData.factories[factoryId].index == -1
     * while in a multiplayer session as a client
     */
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ACH_BroadcastStar.OnGameTick))]
    private static IEnumerable<CodeInstruction> OnGameTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        try
        {
            var codeMatcher = new CodeMatcher(instructions, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "gameData"),
                    new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "factories"),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Ldelem_Ref)
                );

            var factoryIdInstruction = codeMatcher.InstructionAt(3);
            var generatorIdInstruction = codeMatcher.InstructionAt(7);

            return codeMatcher
                .CreateLabel(out var label)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(factoryIdInstruction)
                .InsertAndAdvance(generatorIdInstruction)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<ACH_BroadcastStar, int, int, bool>>(
                    (instance, factoryId, generatorId) =>
                    {
                        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
                        {
                            return true;
                        }

                        if (instance.gameData.factories[factoryId]?.powerSystem?.genPool[generatorId] == null ||
                            instance.gameData.factories[factoryId].index == -1)
                        {
                            return false;
                        }

                        return true;
                    }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ret))
                .InstructionEnumeration();
        }
        catch
        {
            Log.Error("ACH_BroadcastStar.OnGameTick_Transpiler failed. Mod version not compatible with game version.");
            return instructions;
        }
    }
}
