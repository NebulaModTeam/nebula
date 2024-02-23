#region

using System;
using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DFSTurretComponent))]
internal class DFSTurretComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFSTurretComponent.TargetIsInRange))]
    public static bool TargetIsInRange_Prefix(ref DFSTurretComponent __instance, EnemyDFHiveSystem hive,
        SkillTargetLocal target, float sensorRange, out float dist2, ref bool __result)
    {
        dist2 = 0;
        if (!Multiplayer.IsActive || target.type != ETargetType.Player) return true;
        if (!Multiplayer.Session.Combat.ActivedStars.Contains(hive.starData.id)) return true;

        // Get the target alive player upos
        var playerId = target.id;
        var players = Multiplayer.Session.Combat.Players;
        if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(playerId, out var index)) return false;
        if (!players[index].isAlive) return false;

        var vectorLF = players[index].uPosition - hive.starData.uPosition;
        var num2 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
        var num3 = hive.orbitRadius - num2;
        if (num3 < 0.0)
        {
            num3 = -num3;
        }
        if (num3 > 4000.0)
        {
            return false;
        }
        var vectorLF2 = vectorLF * (hive.orbitRadius / num2) + hive.starData.uPosition;
        hive.sector.InverseTransformToAstro_ref(hive.hiveAstroId, ref vectorLF2, out vectorLF2);
        ref var ptr = ref hive.sector.enemyPool[__instance.enemyId];
        dist2 = Vector3.SqrMagnitude(vectorLF2 - ptr.pos);
        __result = dist2 <= (sensorRange * sensorRange);

        if (__result)
        {
            // Assign playerSkillTargetU to later used by Shoot_Plasma
            GameMain.spaceSector.skillSystem.playerSkillTargetU = players[index].skillTargetU;
            // Assign player_local_pos to later used in GetAimTargetDir
            hive.sector.InverseTransformToAstro_ref(hive.hiveAstroId, ref players[index].uPosition, out var player_local_pos);
            hive.player_local_pos = player_local_pos;
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFSTurretComponent.SearchTarget))]
    public static bool SearchTarget(ref DFSTurretComponent __instance, EnemyDFHiveSystem hive,
        EAggressiveLevel aggressiveLevel, ref bool __result)
    {
        if (!Multiplayer.IsActive || aggressiveLevel <= EAggressiveLevel.Passive) return true;

        var sector = hive.sector;
        ref var ptr = ref sector.enemyPool[__instance.enemyId];
        var etargetType = ETargetType.None;
        var targetId = 0;
        var sqrDistToTarget = float.MaxValue;
        var sqrRealAttackRange = __instance.realAttactRange * __instance.realAttactRange;

        var starId = hive.starData.id;
        if (Multiplayer.Session.Combat.ActivedStars.Contains(starId))
        {
            // Find the closest alive player to the turret
            var sqrOrbitRadius = (float)(hive.orbitRadius * hive.orbitRadius);
            var cloestIndex = 0;
            var cloestDiff = float.MaxValue;
            var players = Multiplayer.Session.Combat.Players;
            for (var i = 0; i < players.Length; i++)
            {
                ref var playerPosition = ref players[i];
                if (playerPosition.starId != starId || !playerPosition.isAlive) continue;
                var sqrPlayerOrbitRadius = Vector3.SqrMagnitude(playerPosition.skillTargetU - hive.starData.uPosition);
                var diff = Math.Abs(sqrPlayerOrbitRadius - sqrOrbitRadius);
                if (diff < cloestDiff)
                {
                    cloestIndex = i;
                    cloestDiff = diff;
                }
            }

            if (cloestDiff < (4000.0f * 4000.0f))
            {
                var vectorLF = players[cloestIndex].uPosition - hive.starData.uPosition;
                var num4 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
                var vectorLF2 = vectorLF * (hive.orbitRadius / num4) + hive.starData.uPosition;
                sector.InverseTransformToAstro_ref(hive.hiveAstroId, ref vectorLF2, out vectorLF2);
                var dx = vectorLF2.x - ptr.pos.x;
                var dy = vectorLF2.y - ptr.pos.y;
                var dz = vectorLF2.z - ptr.pos.z;
                var sqrDist = (float)(dx * dx + dy * dy + dz * dz);
                var coef = ((hive.hatred.max.targetType == ETargetType.Player) ? 1f : 0.64f);
                if (sqrDist <= sqrRealAttackRange * coef && sqrDist < sqrDistToTarget)
                {
                    etargetType = ETargetType.Player;
                    targetId = players[cloestIndex].id; // Set to playerId
                    sqrDistToTarget = sqrDist;
                }
            }
        }
        var craftPool = sector.craftPool;
        var craftCursor = sector.craftCursor;
        for (var i = 1; i < craftCursor; i++)
        {
            ref var craftPtr = ref craftPool[i];
            if (craftPtr.id == i && craftPtr.astroId == hive.starData.astroId && !craftPtr.isInvincible)
            {
                sector.TransformFromAstro_ref(craftPtr.astroId, out var craftUpos, ref craftPtr.pos);
                var vectorLF4 = craftUpos - hive.starData.uPosition;
                var dist = Math.Sqrt(vectorLF4.x * vectorLF4.x + vectorLF4.y * vectorLF4.y + vectorLF4.z * vectorLF4.z);
                var orbitDiff = hive.orbitRadius - dist;
                if (orbitDiff < 0.0)
                {
                    orbitDiff = -orbitDiff;
                }
                if (orbitDiff < 4000.0)
                {
                    var pos = vectorLF4 * (hive.orbitRadius / dist) + hive.starData.uPosition;
                    sector.InverseTransformToAstro_ref(hive.hiveAstroId, ref pos, out pos);
                    var dx = pos.x - ptr.pos.x;
                    var dy = pos.y - ptr.pos.y;
                    var dz = pos.z - ptr.pos.z;
                    var sqrDist = (float)(dx * dx + dy * dy + dz * dz);
                    if (sqrDist <= sqrRealAttackRange && sqrDist < sqrDistToTarget)
                    {
                        etargetType = ETargetType.Craft;
                        targetId = craftPtr.id;
                        sqrDistToTarget = sqrDist;
                    }
                }
            }
        }
        if (targetId > 0)
        {
            if (__instance.target.type != etargetType || __instance.target.id != targetId)
            {
                __instance.target.type = etargetType;
                __instance.target.id = targetId;
                __instance.state = EDFTurretState.Aiming;
                var sqrMagnitude = __instance.localDir.sqrMagnitude;
                var scale = Mathf.Sqrt(sqrDistToTarget / sqrMagnitude);
                __instance.localDir.x *= scale;
                __instance.localDir.y *= scale;
                __instance.localDir.z *= scale;
            }
            __result = true;
            return false;
        }
        __result = false;
        return false;
    }
}
