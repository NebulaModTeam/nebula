#region

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;
using UnityEngine;

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


            //  Insert null guard in EnemyUnitComponent loop
            //  if (ptr10.id == k)
            //  {
            //      ref EnemyData ptr11 = ref enemyPool[ptr10.enemyId];
            //      PrefabDesc prefabDesc = SpaceSector.PrefabDescByModelIndex[(int)ptr11.modelIndex];
            //      if (prefabDesc == null) continue;  // null guard

            codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyUnitComponent), nameof(EnemyUnitComponent.id))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Bne_Un));
            if (codeMatcher.IsInvalid)
            {
                Log.Warn("EnemyDFHiveSystem.GameTickLogic: Can't find operand_continue");
                return codeMatcher.InstructionEnumeration();
            }
            var operand_continue = codeMatcher.Operand;

            codeMatcher.MatchForward(true,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(SpaceSector), nameof(SpaceSector.PrefabDescByModelIndex))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldelem_Ref),
                new CodeMatch(OpCodes.Stloc_S));
            if (codeMatcher.IsInvalid)
            {
                Log.Warn("EnemyDFHiveSystem.GameTickLogic: Can't find operand_prefabDesc");
                return codeMatcher.InstructionEnumeration();
            }
            var operand_prefabDesc = codeMatcher.Operand;

            codeMatcher.Advance(1).Insert(
                new CodeInstruction(OpCodes.Ldloc_S, operand_prefabDesc),
                new CodeInstruction(OpCodes.Brfalse_S, operand_continue));

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

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.AssaultingWavesDetermineAI))]
    public static IEnumerable<CodeInstruction> AssaultingWavesDetermineAI_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Launch assault for player who on remote planet and has no power buildings
            from:
				if (!flag2 && this.gameData.localPlanet != null && this.gameData.localPlanet.type != EPlanetType.Gas && this.gameData.localPlanet.star == this.starData)
				{
					flag2 = true;
					num5 = this.gameData.localPlanet.astroId;
					vector2 = (vector = this.sector.skillSystem.playerSkillTargetL);
				}
				if (flag2) {
                    ...
                    this.LaunchLancerAssault(aggressiveLevel, vector, vector2, num5, num2, num15);
                }
            to:
				if (!flag2 && this.gameData.localPlanet != null && this.gameData.localPlanet.type != EPlanetType.Gas && this.gameData.localPlanet.star == this.starData)
				{
					flag2 = true;
					num5 = this.gameData.localPlanet.astroId;
					vector2 = (vector = this.sector.skillSystem.playerSkillTargetL);
				}
			>>	if (LaunchCondition(flag2, this, ref num5, ref vector, ref vector2))
                {
                    ...
                    this.LaunchLancerAssault(aggressiveLevel, vector, vector2, num5, num2, num15);
                }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(true,
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Stloc_S), // flag2 = true
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Stloc_S), // num5 = this.gameData.localPlanet.astroId
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(OpCodes.Dup),
                    new CodeMatch(OpCodes.Stloc_S), // vector
                    new CodeMatch(OpCodes.Stloc_S), // vector2
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Brfalse));

            if (codeMatcher.IsInvalid)
            {
                Log.Warn("EnemyDFHiveSystem.AssaultingWavesDetermineAI: Can't find target");
                return codeMatcher.InstructionEnumeration();
            }
            var tarPos = codeMatcher.InstructionAt(-2).operand;
            var maxHatredPos = codeMatcher.InstructionAt(-3).operand;
            var targetAstroId = codeMatcher.InstructionAt(-9).operand;

            codeMatcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloca_S, targetAstroId),
                new CodeInstruction(OpCodes.Ldloca_S, tarPos),
                new CodeInstruction(OpCodes.Ldloca_S, maxHatredPos),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnemyDFHiveSystem_Transpiler), nameof(LaunchCondition))));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Warn("Transpiler EnemyDFHiveSystem.AssaultingWavesDetermineAI failed.");
            Log.Warn(e);
            return instructions;
        }
    }

    static bool LaunchCondition(bool originalFlag, EnemyDFHiveSystem @this, ref int targetAstroId, ref Vector3 tarPos, ref Vector3 maxHatredPos)
    {
        if (!Multiplayer.IsActive || originalFlag == true) return originalFlag;

        var players = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < players.Length; i++)
        {
            if (players[i].isAlive && players[i].starId == @this.starData.id && players[i].planetId > 0)
            {
                var planet = GameMain.galaxy.PlanetById(players[i].planetId);
                if (planet == null || planet.type == EPlanetType.Gas) continue;

                targetAstroId = players[i].planetId;
                tarPos = maxHatredPos = players[i].skillTargetL;
                Log.Info($"Hive attack LaunchCondition: player[{i}] planeId{targetAstroId}");
                return true;
            }
        }
        return originalFlag;
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
