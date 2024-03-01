#region

using System;
using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#pragma warning disable IDE1006
#pragma warning disable IDE0007
#pragma warning disable IDE0018

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyUnitComponent))]
internal class EnemyUnitComponent_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyUnitComponent.GetTargetPosition_Ground))]
    public static void GetTargetPosition_Ground_Postfix(int idType, ref Vector3 target, ref bool __result)
    {
        if (!Multiplayer.IsActive) return;
        var targetType = idType >> 26;
        var targetId = idType & 67108863;
        if (targetType == 15) //player
        {
            var players = Multiplayer.Session.Combat.Players;
            for (var i = 0; i < players.Length; i++)
            {
                if (players[i].id == targetId)
                {
                    target = players[i].position;
                    __result = players[i].isAlive;
                    return;
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyUnitComponent.GetTargetPosition_Space))]
    public static bool GetTargetPosition_Space_Postfix(int idType, ref VectorLF3 target, ref float territory, ref bool __result)
    {
        if (!Multiplayer.IsActive) return true;

        var targetType = idType >> 26;
        var targetId = idType & 67108863;
        if (targetType == 15) //player
        {
            if (!Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(targetId, out var index))
            {
                __result = false;
                return false;
            }

            ref var ptr = ref Multiplayer.Session.Combat.Players[index];
            target = ptr.uPosition;
            territory = GameMain.mainPlayer.mecha.energyShieldRadius; // Currently the shieldRadius are same for all mecha
            __result = ptr.isAlive;
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyUnitComponent.SensorLogic_Ground))]
    static void SensorLogic_Ground_Postfix(ref EnemyUnitComponent __instance, ref EnemyData enemy, PlanetFactory factory, int hatred_max_sense, EAggressiveLevel aggressiveLevel)
    {
        if (!Multiplayer.IsActive) return;

        var planetId = __instance.planetId;
        if (!Multiplayer.Session.Combat.ActivedPlanets.Contains(planetId)) return;

        var players = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < players.Length; i++)
        {
            if (players[i].planetId != planetId)
            {
                continue;
            }
            // Skip assaults.targetObject for now

            var num = 1.0f;
            switch (aggressiveLevel)
            {
                case EAggressiveLevel.Dummy:
                    return;
                case EAggressiveLevel.Passive:
                    return;
                case EAggressiveLevel.Torpid:
                    num = 0.85f;
                    break;
                case EAggressiveLevel.Normal:
                    num = 1.0f;
                    break;
                case EAggressiveLevel.Sharp:
                    num = 1.2f;
                    break;
                case EAggressiveLevel.Rampage:
                    num = 1.4f;
                    break;
            }

            var prefabDescByModelIndex = SpaceSector.PrefabDescByModelIndex;
            var prefabDesc = prefabDescByModelIndex[enemy.modelIndex];
            var num2 = prefabDesc.unitSensorRange * num;
            var num4 = num2 * 1.45f + factory.skillSystem.playerEnergyShieldRadius;
            var num6 = (float)enemy.pos.x;
            var num7 = (float)enemy.pos.y;
            var num8 = (float)enemy.pos.z;


            // Find the nearest alive player to enemy unit
            var index = 0;
            var distance = float.MaxValue;
            var playerId = -1;
            for (var j = 0; j < Multiplayer.Session.Combat.Players.Length; j++)
            {
                ref var player = ref Multiplayer.Session.Combat.Players[j];
                if (player.planetId != planetId || !player.isAlive)
                {
                    continue;
                }
                var dx = player.position.x - num6;
                var dy = player.position.y - num7;
                var dz = player.position.z - num8;
                var sqtDist = dx * dx + dy * dy + dz * dz;
                if (sqtDist < distance)
                {
                    index = j;
                    distance = sqtDist;
                    playerId = player.id;
                }
            }
            // If there is no alive player, return the original value
            if (playerId == -1) return;


            var num39 = distance;
            ref var ptr3 = ref Multiplayer.Session.Combat.Players[index].position;
            var num26 = 0f;
            if (__instance.assaults.count > 0)
            {
                var num27 = ptr3.x - __instance.assaults.target.x;
                var num28 = ptr3.y - __instance.assaults.target.y;
                var num29 = ptr3.z - __instance.assaults.target.z;
                var num30 = Mathf.Sqrt(num27 * num27 + num28 * num28 + num29 * num29);
                num26 = 1f - Mathf.Clamp01((num30 - 40f) / (40f * num + (__instance.level * 2) * num));
            }
            var num31 = ptr3.x - num6;
            var num32 = ptr3.y - num7;
            var num33 = ptr3.z - num8;
            var num34 = Mathf.Sqrt(num31 * num31 + num32 * num32 + num33 * num33);
            var num35 = 1f - Mathf.Clamp01((num34 - 60f) / (40f * num + (__instance.level * 4) * num));
            if (num35 > num26)
            {
                num26 = num35;
            }
            if (num26 > 0.1f && num39 < num4 * num4)
            {
                var num17 = (num4 - Mathf.Sqrt(num39)) * 0.7f + 7f;
                num17 *= num26;
                if (num17 > 0f)
                {
                    if (num17 < 8f)
                    {
                        num17 = 8f;
                    }
                    __instance.hatred.HateTarget(ETargetType.Player, playerId, (int)num17, hatred_max_sense, EHatredOperation.Add);
                }
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyUnitComponent.SensorLogic_Space))]
    public static bool SensorLogic_Space(ref EnemyUnitComponent __instance, ref EnemyData enemy, SpaceSector sector, EnemyDFHiveSystem hive, int hatred_max_sense, EAggressiveLevel aggressiveLevel)
    {
        if (!Multiplayer.IsActive) return true;

        if (__instance.hatred.max.notNull && __instance.hatred.max.value < sector.skillSystem.maxHatredSpaceTmp * 0.7f)
        {
            enemy.counterAttack = false;
        }
        float aggressiveCoef;
        switch (aggressiveLevel)
        {
            case EAggressiveLevel.Rampage: aggressiveCoef = 1.55f; break;
            case EAggressiveLevel.Sharp: aggressiveCoef = 1.25f; break;
            case EAggressiveLevel.Normal: aggressiveCoef = 1.0f; break;
            case EAggressiveLevel.Torpid: aggressiveCoef = 0.8f; break;
            default: return false;
        }
        var prefabDesc = SpaceSector.PrefabDescByModelIndex[enemy.modelIndex];
        var sensorRnage = prefabDesc.unitSensorRange * aggressiveCoef;
        CraftSearchLogic(ref __instance, ref enemy, hive, hatred_max_sense, sensorRnage);
        PlayerSearchLogic(ref __instance, ref enemy, hive, hatred_max_sense, sensorRnage);
        return false;
    }

    private static void CraftSearchLogic(ref EnemyUnitComponent @this, ref EnemyData enemy,
        EnemyDFHiveSystem hive, int hatred_max_sense, float sensorRange)
    {
        double x = enemy.pos.x;
        double y = enemy.pos.y;
        double z = enemy.pos.z;

        SpaceSector sector = hive.sector;

        var sqrSensorRange = sensorRange * sensorRange;
        float num6 = 6000f;

        CraftData[] craftPool = sector.craftPool;
        int craftCursor = sector.craftCursor;
        for (int i = 1; i < craftCursor; i++)
        {
            ref CraftData ptr = ref craftPool[i];
            if (ptr.id == i && ptr.astroId == hive.starData.astroId && !ptr.isInvincible)
            {
                if (enemy.astroId == hive.hiveAstroId) // enemy is in the hive
                {
                    VectorLF3 vectorLF;
                    sector.TransformFromAstro_ref(ptr.astroId, out vectorLF, ref ptr.pos);
                    VectorLF3 vectorLF2 = vectorLF - hive.starData.uPosition;
                    double num7 = Math.Sqrt(vectorLF2.x * vectorLF2.x + vectorLF2.y * vectorLF2.y + vectorLF2.z * vectorLF2.z);
                    double num8 = hive.orbitRadius - num7;
                    if (num8 < 0.0)
                    {
                        num8 = -num8;
                    }
                    if (num8 <= (double)num6)
                    {
                        VectorLF3 vectorLF3 = vectorLF2 * (hive.orbitRadius / num7) + hive.starData.uPosition;
                        sector.InverseTransformToAstro_ref(hive.hiveAstroId, ref vectorLF3, out vectorLF3);
                        double num9 = vectorLF3.x - x;
                        double num10 = vectorLF3.y - y;
                        double num11 = vectorLF3.z - z;
                        float num12 = (float)(num9 * num9 + num10 * num10 + num11 * num11);
                        if (num12 <= sqrSensorRange)
                        {
                            double num13 = (double)sqrSensorRange - Math.Sqrt((double)num12);
                            if (@this.protoId == 8113)
                            {
                                if (vectorLF3.x * vectorLF3.x + vectorLF3.y * vectorLF3.y + vectorLF3.z * vectorLF3.z > 400000000.0)
                                {
                                    continue;
                                }
                            }
                            else if (@this.protoId == 8112)
                            {
                                VectorLF3 pos;
                                sector.InverseTransformToAstro_ref(enemy.astroId, ref vectorLF, out pos);
                                double num14 = Math.Sqrt(pos.x * pos.x + pos.y * pos.y + pos.z * pos.z);
                                float num15 = 1f - Mathf.Clamp01((float)num14 / 5500f);
                                num13 *= (double)num15;
                            }
                            if (num13 > 0.0)
                            {
                                @this.hatred.HateTarget(ETargetType.Craft, i, (int)num13, hatred_max_sense, EHatredOperation.Add);
                            }
                        }
                    }
                }
                else if (enemy.astroId == hive.starData.astroId) // Replace localStar with hive.starData
                {
                    VectorLF3 pos;
                    if (ptr.astroId == enemy.astroId)
                    {
                        pos = ptr.pos;
                    }
                    else
                    {
                        VectorLF3 vectorLF;
                        sector.TransformFromAstro_ref(ptr.astroId, out vectorLF, ref ptr.pos);
                        sector.InverseTransformToAstro_ref(enemy.astroId, ref vectorLF, out pos);
                    }
                    double x2 = pos.x;
                    double y2 = pos.y;
                    double z2 = pos.z;
                    double num16 = x2 * x2 + y2 * y2 + z2 * z2;
                    double num17 = (double)hive.starData.systemRadius * 40000.0;
                    if (num16 > num17 * num17 * 2.25)
                    {
                        if (@this.hatred.max.targetType == ETargetType.Craft)
                        {
                            @this.hatred.ClearMax();
                        }
                        return;
                    }
                    double num18 = x2 - x;
                    double num19 = y2 - y;
                    double num20 = z2 - z;
                    double num21 = num18 * num18 + num19 * num19 + num20 * num20;
                    if (num21 < (double)sqrSensorRange)
                    {
                        double num13 = (double)sqrSensorRange - Math.Sqrt(num21);
                        if (num13 > 0.0)
                        {
                            @this.hatred.HateTarget(ETargetType.Craft, i, (int)num13, hatred_max_sense, EHatredOperation.Add);
                        }
                    }
                }
            }
        }
    }

    private static void PlayerSearchLogic(ref EnemyUnitComponent @this, ref EnemyData enemy,
        EnemyDFHiveSystem hive, int hatred_max_sense, float sensorRange)
    {
        var starId = hive.starData.id;
        // Only search if there are players in space
        if (!Multiplayer.Session.Combat.ActivedStarsMechaInSpace.Contains(starId)) return;

        SpaceSector sector = hive.sector;
        var sqrSensorRange = sensorRange * sensorRange;
        var reachRange = sensorRange * 1.45f + sector.skillSystem.playerEnergyShieldRadius;

        // Convert enemy local pos to upos
        sector.TransformFromAstro_ref(enemy.astroId, out var enemyUpos, ref enemy.pos);

        // Find the closest alive player to the unit
        var closestPlayerId = -1;
        var cloestSqrDist = float.MaxValue;
        var players = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < players.Length; i++)
        {
            ref var playerPosition = ref players[i];
            if (playerPosition.starId != starId || !playerPosition.isAlive) continue;
            var sqrDist = Vector3.SqrMagnitude(playerPosition.skillTargetU - enemyUpos);
            if (sqrDist < cloestSqrDist)
            {
                closestPlayerId = playerPosition.id;
                cloestSqrDist = sqrDist;
            }
        }

        if (closestPlayerId == -1) // No alive player in the system
        {
            if (@this.hatred.max.targetType == ETargetType.Player)
            {
                @this.hatred.ClearMax();
            }
            return;
        }

        if (enemy.astroId == hive.hiveAstroId) // Enemy is in the hive
        {
            var sensorCoef = ((@this.hatred.max.targetType == ETargetType.Player) ? 1f : 0.64f);
            if (cloestSqrDist > sqrSensorRange * sensorCoef) return;
            var closestDist = (float)Math.Sqrt(cloestSqrDist);
            var hateValue = sensorRange - closestDist;
            if (@this.protoId == 8113) // Lancer
            {
                if (cloestSqrDist > 400000000.0) return;
            }
            else if (@this.protoId == 8112) // Humpback
            {
                hateValue *= 1f - Mathf.Clamp01(closestDist / 5500f);
            }
            if (hateValue > 0.0)
            {
                @this.hatred.HateTarget(ETargetType.Player, closestPlayerId, (int)hateValue, hatred_max_sense, EHatredOperation.Add);
                return;
            }
        }
        else if (enemy.astroId == hive.starData.astroId) // Enemy is in the system of the hive
        {
            if (cloestSqrDist > reachRange * reachRange) return;
            var closestDist = (float)Math.Sqrt(cloestSqrDist);
            var hateValue = reachRange - closestDist;
            if (hateValue > 0.0)
            {
                @this.hatred.HateTarget(ETargetType.Player, closestPlayerId, (int)hateValue, hatred_max_sense, EHatredOperation.Add);
                return;
            }
        }
    }
}
