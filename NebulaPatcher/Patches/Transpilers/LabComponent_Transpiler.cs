#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(LabComponent))]
internal class LabComponent_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(LabComponent.InternalUpdateResearch))]
    private static IEnumerable<CodeInstruction> InternalUpdateResearch_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Change: if (ts.hashUploaded >= ts.hashNeeded)
        //To:     if (ts.hashUploaded >= ts.hashNeeded && (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        try
        {
            var matcher = new CodeMatcher(codeInstructions)
                .MatchForward(true,
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TechState), nameof(TechState.hashUploaded))),
                    new CodeMatch(i => i.IsLdarg()),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TechState), nameof(TechState.hashNeeded))),
                    new CodeMatch(OpCodes.Blt) //IL 339
                );
            var label = matcher.Instruction.operand;
            matcher.Advance(1)
                .InsertAndAdvance(HarmonyLib.Transpilers.EmitDelegate(() =>
                    !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse_S, label));
            return matcher.InstructionEnumeration();
        }
        catch
        {
            Log.Error("LabComponent.InternalUpdateResearch_Transpiler failed. Mod version not compatible with game version.");
            return codeInstructions;
        }
    }
}
