#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(EnemyDFHiveSystem))]
internal class EnemyDFHiveSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.GameTickLogic))]
    public static IEnumerable<CodeInstruction> GameTickLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Suppress RealizePlanetBase in client
            from:
				if ((i + this.hiveAstroId) % 5 == num5 && dfrelayComponent.targetAstroId == num4 && dfrelayComponent.baseState == 1 && dfrelayComponent.stage == 2)
				{
					dfrelayComponent.RealizePlanetBase(this.sector);
				}
            to:
				if ((i + this.hiveAstroId) % 5 == num5 && dfrelayComponent.targetAstroId == num4 && dfrelayComponent.baseState == 1 && dfrelayComponent.stage == 2)
				{
					EnemyDFHiveSystem_Transpiler.RealizePlanetBase(dfrelayComponent, this.sector);
				}
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(DFRelayComponent), nameof(DFRelayComponent.RealizePlanetBase))))
                .Set(OpCodes.Call, AccessTools.Method(typeof(EnemyDFHiveSystem_Transpiler), nameof(RealizePlanetBase)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler EnemyDFHiveSystem.GameTickLogic failed.");
            Log.Error(e);
            return instructions;
        }
    }

    static void RealizePlanetBase(DFRelayComponent dFRelayComponent, SpaceSector spaceSector)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer)
        {
            dFRelayComponent.RealizePlanetBase(spaceSector);
        }
    }
}
