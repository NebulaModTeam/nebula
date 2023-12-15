#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(BuildTool_Path))]
internal class BuildTool_Path_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BuildTool_Path.CreatePrebuilds))]
    private static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, il)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_controller"),
                new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(PlayerController), nameof(PlayerController.cmd))),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(CommandState), nameof(CommandState.stage))));

        if (!matcher.IsInvalid)
        {
            return matcher
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                    Multiplayer.IsActive && Multiplayer.Session.Factories.IsIncomingRequest.Value &&
                    Multiplayer.Session.Factories.PacketAuthor != Multiplayer.Session.LocalPlayer.Id))
                .CreateLabelAt(matcher.Pos + 19 + 22, out var jmpLabel)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jmpLabel))
                .InstructionEnumeration();
        }
        Log.Error("BuildTool_Path.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
        return codeInstructions;

    }
}
