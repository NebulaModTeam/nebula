#region

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

/*
 * this one is part of the smoother planet loading
 * it is REALLY essential
 * normally the game would not update the planet simulation while the factory is loading in.
 * this transpiler makes sure that the simulation goes on while the factory is still loading
 * it is a fix for the weird planet movement we had in the early days ;)
 */
[HarmonyPatch(typeof(PlanetSimulator))]
internal class PlanetSimulator_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlanetSimulator.UpdateUniversalPosition))]
    public static IEnumerable<CodeInstruction> UpdateUniversalPosition_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        instructions = new CodeMatcher(instructions)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetSimulator), "planetData")),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(PlanetData), "factoryLoading")),
                new CodeMatch(OpCodes.Brfalse))
            .SetAndAdvance(OpCodes.Nop, null)
            .SetAndAdvance(OpCodes.Nop, null)
            .SetAndAdvance(OpCodes.Nop, null)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .Insert(HarmonyLib.Transpilers.EmitDelegate<Func<PlanetSimulator, bool>>(_ => false))
            .InstructionEnumeration();
        return instructions;
    }
}
