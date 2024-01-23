#region

using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyUnitComponent))]
internal class EnemyUnitComponent_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyUnitComponent.GetTargetPosition_Ground))]
    public static void GetTargetPosition_Ground_Postfix(int idType, ref Vector3 target, ref bool __result)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }
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
                    __result = true;
                    return;
                }
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyUnitComponent.SensorLogic_Ground))]
    static void SensorLogic_Ground_Postfix(ref EnemyUnitComponent __instance, ref EnemyData enemy, PlanetFactory factory, int hatred_max_sense, EAggressiveLevel aggressiveLevel)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        var planetId = __instance.planetId;
        if (!Multiplayer.Session.Combat.ActivedPlanets.Contains(planetId))
        {
            return;
        }

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


            // Find the nearest player to enemy unit
            var index = 0;
            var distance = float.MaxValue;
            var playerId = 0;
            for (var j = 0; j < Multiplayer.Session.Combat.Players.Length; j++)
            {
                ref var player = ref Multiplayer.Session.Combat.Players[j];
                if (player.planetId != planetId)
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
}
