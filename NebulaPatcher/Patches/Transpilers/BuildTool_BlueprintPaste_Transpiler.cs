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

[HarmonyPatch(typeof(BuildTool_BlueprintPaste))]
internal class BuildTool_BlueprintPaste_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BuildTool_BlueprintPaste.CreatePrebuilds))]
    private static IEnumerable<CodeInstruction> CreatePrebuilds_Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        /*
         * Inserts
         *  if(!Multiplayer.IsActive)
         * Before trying to take items, so that all prebuilds are assumed to require items while in MP
         */
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, il)
            .MatchForward(true,
                new CodeMatch(i => i.IsLdloc()), // count
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Ceq),
                new CodeMatch(OpCodes.Brfalse)
            );

        if (matcher.IsInvalid)
        {
            Log.Error(
                "BuildTool_BlueprintPaste.CreatePrebuilds_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        var jumpOperand = matcher.Instruction.operand;

        matcher = matcher
            .MatchBack(false,
                new CodeMatch(i => i.IsLdloc()),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "item"),
                new CodeMatch(i => i.opcode == OpCodes.Ldfld && ((FieldInfo)i.operand).Name == "ID"),
                new CodeMatch(i => i.IsStloc())
            );

        if (!matcher.IsInvalid)
        {
            return matcher
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() => Multiplayer.IsActive))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, jumpOperand))
                .InstructionEnumeration();
        }
        Log.Error(
            "BuildTool_BlueprintPaste.CreatePrebuilds_Transpiler 2 failed. Mod version not compatible with game version.");
        return codeInstructions;
    }
}
