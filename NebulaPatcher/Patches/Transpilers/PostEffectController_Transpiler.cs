#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PostEffectController))]
internal class PostEffectController_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PostEffectController.Update))]
    private static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    {
        // TODO:   Find out the root cause of fast travel error
        // Change: if (localPlanet.type != EPlanetType.Gas)
        // To:     if (localPlanet.type != EPlanetType.Gas && localPlanet.factoryloaded)

        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var matcher = new CodeMatcher(codeInstructions, il)
            .MatchForward(true,
                new CodeMatch(i => i.IsLdloc()),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), nameof(PlanetData.type))),
                new CodeMatch(OpCodes.Ldc_I4_5),
                new CodeMatch(OpCodes.Beq)
            );

        if (matcher.IsInvalid)
        {
            Log.Error("PostEffectController.Update_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }

        var jumpOperand = matcher.Instruction.operand;

        return matcher
            .Advance(1)
            .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() => GameMain.localPlanet.factoryLoaded))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, jumpOperand))
            .InstructionEnumeration();
    }
}
