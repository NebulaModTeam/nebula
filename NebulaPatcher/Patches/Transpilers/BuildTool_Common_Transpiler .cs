#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch]
internal class BuildTool_Common_Transpiler
{
    /*
     * Replaces
     *  int num;
     *  if (@this.player.inhandItemId == id && @this.player.inhandItemCount > 0)
     * With
     *  int num = 1;
     *  if (@this.player.inhandItemId == id && @this.player.inhandItemCount > 0)
     * So that it succeeds when processing another player's request
     */
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
    [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Inserter.CreatePrebuilds))]
    private static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions)
            .MatchForward(false,
                new CodeMatch(i => i.IsLdarg()),
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_player"),
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_inhandItemId"));

        if (codeMatcher.IsInvalid)
        {
            Log.Error("BuildTool_Common.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        // num = 1; from within the if statement
        var numInstruction = codeMatcher.InstructionAt(11);

        return codeMatcher
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
            .InsertAndAdvance(numInstruction)
            .InstructionEnumeration();
    }
}
