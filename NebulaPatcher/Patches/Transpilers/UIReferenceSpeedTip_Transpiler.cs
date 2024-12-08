#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(UIReferenceSpeedTip))]
public static class UIReferenceSpeedTip_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(UIReferenceSpeedTip.RefreshSubEntries))]
    private static IEnumerable<CodeInstruction> RefreshSubEntries_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Remove planetData.factory != null condiction check
        //Change: if (planetData != null && planetData.factory != null && this.loadedSubTipDatas[planetData.astroId].astroId == planetData.astroId)
        //To:     if (planetData != null && this.loadedSubTipDatas[planetData.astroId].astroId == planetData.astroId)
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            return new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(PlanetData), nameof(PlanetData.factory))),
                    new CodeMatch(OpCodes.Brfalse)
                )
                .Repeat(matcher => matcher
                    .SetAndAdvance(OpCodes.Pop, null)
                )
                .InstructionEnumeration();
        }
        catch
        {
            Log.Error("Transpiler UIReferenceSpeedTip.RefreshSubEntries failed. Reference speed tip may not work properly.");
            return codeInstructions;
        }
    }
}
