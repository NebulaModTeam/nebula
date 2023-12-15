#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(PlayerController))]
internal class PlayerController_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerController.RigidbodySafer))]
    public static IEnumerable<CodeInstruction> RigidbodySafer_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Change: if (this.gameData.localPlanet != null)
        // To:     if (this.gameData.localPlanet != null && this.gameData.localPlanet.loaded)

        var codeInstructions = instructions as CodeInstruction[] ?? instructions.ToArray();
        var codeMatcher = new CodeMatcher(codeInstructions)
            .MatchForward(false,
                new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_localPlanet"),
                new CodeMatch(i => i.opcode == OpCodes.Brfalse)
            );

        if (!codeMatcher.IsInvalid)
        {
            return codeMatcher
                .SetInstruction(
                    HarmonyLib.Transpilers.EmitDelegate<Func<GameData, bool>>(gamedata => gamedata.localPlanet is
                    { loaded: true }))
                .InstructionEnumeration();
        }
        Log.Error("PlayerController_Transpiler.RigidbodySafer failed. Mod version not compatible with game version.");
        return codeInstructions;
    }
}
