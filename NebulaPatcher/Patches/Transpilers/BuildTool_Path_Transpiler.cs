using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    [HarmonyPatch(typeof(BuildTool_Path))]
    class BuildTool_Path_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BuildTool_Path.CreatePrebuilds))]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, il)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_controller"),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerController), nameof(PlayerController.cmd))),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(CommandState), nameof(CommandState.stage))));

            if (matcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildTool_Path.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
                return instructions;
            }

            return matcher
                    .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate<Func<bool>>(() =>
                    {
                        return Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest && Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id;
                    }))
                    .CreateLabelAt(matcher.Pos + 19, out Label jmpLabel)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpLabel))
                    .InstructionEnumeration();
        }
    }
}
