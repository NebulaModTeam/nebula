#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.SpaceEnemy;
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

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.KeyTickLogic))]
    public static IEnumerable<CodeInstruction> KeyTickLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Sync RemoveEnemyFinal of tinders
            from:
		        this.sector.RemoveEnemyFinal(buffer7[num5].enemyId);
            to:
		        EnemyDFHiveSystem_Transpiler.RemoveEnemyFinal(this.sector, buffer7[num5].enemyId);
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(SpaceSector), nameof(SpaceSector.RemoveEnemyFinal))))
                .Set(OpCodes.Call, AccessTools.Method(typeof(EnemyDFHiveSystem_Transpiler), nameof(RemoveEnemyFinal)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler EnemyDFHiveSystem.KeyTickLogic failed.");
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

    static void RemoveEnemyFinal(SpaceSector spaceSector, int enemyId)
    {
        if (enemyId <= 0) return;
        if (Multiplayer.IsActive)
        {
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Network.SendPacket(new DFSRemoveEnemyDeferredPacket(enemyId));
            }
            else
            {
                // Don't remove on client
                return;
            }
        }
        spaceSector.RemoveEnemyFinal(enemyId);
    }
}
