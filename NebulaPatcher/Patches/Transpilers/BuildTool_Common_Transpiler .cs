using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpiler
{
    class BuildTool_Common_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.CreatePrebuilds))]
        [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.CreatePrebuilds))]
        static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Set int count = 1 before trying to use hand items or take tail items so that it passes if player did not generate event
            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "get_player"),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_inhandItemId"));

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("BuildTool_Common.CreatePrebuilds_Transpiler failed");
                return instructions;
            }

            return codeMatcher
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1))
                    .InstructionEnumeration();
        }
    }
}
