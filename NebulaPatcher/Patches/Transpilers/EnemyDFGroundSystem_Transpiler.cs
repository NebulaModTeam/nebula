#region

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Transpilers;

[HarmonyPatch(typeof(EnemyDFGroundSystem))]
internal class EnemyDFGroundSystem_Transpiler
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.GameTickLogic))]
    public static IEnumerable<CodeInstruction> GameTickLogic_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Overwrite the condition of local player and position in MP
                First make it skips the original logic that only work for singleplayer, by setting local_player_exist = false
            from:
                if (isLocalLoaded) {
                    Player mainPlayer = this.gameData.mainPlayer;
			        this.local_player_pos = mainPlayer.position;
			        this.local_player_pos_lf = mainPlayer.position; // use by UpdateFactoryThreat, which will not execute if local_player_total_energy_consume = 0
			        this.local_player_exist = true;                 // only inside this function
			        this.local_player_alive = mainPlayer.isAlive;   // only inside this function
			        this.local_player_exist_alive = this.local_player_exist && this.local_player_alive;
			        this.local_player_grounded_alive = this.local_player_exist_alive && !mainPlayer.sailing;
			        this.local_player_total_energy_consume = (long)(mainPlayer.mecha.totalEnergyConsume + 0.5); // use in KeyTickLogic
                }
                else {
                    ...
                }
            to:
                if (isLocalLoaded && !Multiplayer.IsActive) {
                    Player mainPlayer = this.gameData.mainPlayer;
                    ...
                }
                else {
                    ...
                }
            */

            var codeMatcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(EnemyDFGroundSystem), nameof(EnemyDFGroundSystem.isLocalLoaded))),
                    new CodeMatch(i => i.IsStloc()),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Brfalse));

            var jumpOperand = codeMatcher.Instruction.operand;
            codeMatcher.Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Call, AccessTools.DeclaredPropertyGetter(typeof(Multiplayer), nameof(Multiplayer.IsActive))),
                    new CodeInstruction(OpCodes.Brtrue_S, jumpOperand)
                );

            /*  Sync max hatred target before executing unit behavior in MP                
            before:
				switch (ptr6.behavior)
				{
                    ...
                }
            insert:
                SyncHatredTarget(this, ptr)
            */

            codeMatcher
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyUnitComponent), nameof(EnemyUnitComponent.behavior))),
                    new CodeMatch(OpCodes.Stloc_S),
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Switch)
                );

            var ptr = codeMatcher.Instruction.operand;
            codeMatcher.Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldloc_S, ptr),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnemyDFGroundSystem_Transpiler), nameof(SyncHatredTarget)))
                );

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler EnemyDFGroundSystem.GameTickLogic failed. Ground DF untis aggro will not in sync.");
            Log.Error(e);
            return instructions;
        }
    }

    private static void SyncHatredTarget(EnemyDFGroundSystem groundSystem, ref EnemyUnitComponent enemyUnit)
    {
        if (!Multiplayer.IsActive) return;

        var planetId = groundSystem.planet.id;
        var targets = Multiplayer.Session.Enemies.GroundTargets[planetId];
        var enemyId = enemyUnit.enemyId;
        if (enemyId >= targets.Length) return;

        if (Multiplayer.Session.IsServer)
        {
            // TODO: Sync fighter drones in future. For now stop enemy unit from targeting craft.
            if (enemyUnit.hatred.max.objectType == EObjectType.Craft)
            {
                enemyUnit.hatred.ClearMax();
            }
            var currentTarget = enemyUnit.hatred.max.target;
            if (targets[enemyId] != currentTarget)
            {
                targets[enemyId] = currentTarget;
                var starId = planetId / 100;
                var packet = new DFGRetargetPacket(planetId, enemyId, currentTarget);
                Multiplayer.Session.Server.SendPacketToStar(packet, starId);
            }
        }
        else
        {
            enemyUnit.hatred.max.target = targets[enemyId]; // overwrite with value from server
            enemyUnit.hatred.max.value = 100000; // dummy max value
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.DeactivateUnit))]
    public static IEnumerable<CodeInstruction> DeactivateUnit_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            /*  Broadcast when removing is success
            from:
                this.factory.RemoveEnemyFinal(enemyId);
            to:
                EnemyDFGroundSystem_Transpiler.RemoveEnemyFinal(this.factory, enemyId)
            */

            var codeMatcher = new CodeMatcher(instructions)
                .End()
                .MatchBack(true, new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "RemoveEnemyFinal"))
                .Set(OpCodes.Call, AccessTools.Method(typeof(EnemyDFGroundSystem_Transpiler), nameof(RemoveEnemyFinal)));

            return codeMatcher.InstructionEnumeration();
        }
        catch (System.Exception e)
        {
            Log.Error("Transpiler DeactivateUnit_Transpiler failed.");
            Log.Error(e);
            return instructions;
        }
    }

    public static void RemoveEnemyFinal(PlanetFactory factory, int id)
    {
        factory.RemoveEnemyFinal(id);
        if (Multiplayer.IsActive && Multiplayer.Session.IsServer)
        {
            var planetId = factory.planetId;
            var starId = factory.planet.star.id;
            var packet = new DFGDeactivateUnitPacket(planetId, id);
            Multiplayer.Session.Server.SendPacketToStar(packet, starId);
        }
    }
}
