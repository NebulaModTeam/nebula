using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NebulaPatcher.Patches.Transpilers
{
    [HarmonyPatch(typeof(PlayerController))]
    internal class PlayerController_Transpiler
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(PlayerController.RigidbodySafer))]
        public static IEnumerable<CodeInstruction> RigidbodySafer_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Change: if (this.gameData.localPlanet != null)
            // To:     if (this.gameData.localPlanet != null && this.gameData.localPlanet.loaded)

            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_localPlanet"),
                    new CodeMatch(i => i.opcode == OpCodes.Brfalse)
                );

            if (codeMatcher.IsInvalid)
            {
                NebulaModel.Logger.Log.Error("PlayerController_Transpiler.RigidbodySafer failed. Mod version not compatible with game version.");
                return instructions;
            }

            return codeMatcher
                .SetInstruction (HarmonyLib.Transpilers.EmitDelegate<Func<GameData, bool>>((gamedata) =>
                {
                    return gamedata.localPlanet != null && gamedata.localPlanet.loaded;
                }))
                .InstructionEnumeration();
        }
    }
}
