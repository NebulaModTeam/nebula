#region

using System.Linq;
using HarmonyLib;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DFGTurretComponent))]
internal class DFGTurretComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGTurretComponent.TargetIsInRange))]
    public static bool TargetIsInRange_Prefix(ref DFGTurretComponent __instance, PlanetFactory factory,
        SkillTargetLocal target, DFGBaseComponent @base, out float dist2, ref bool __result)
    {
        dist2 = 0;
        if (!Multiplayer.IsActive || !Multiplayer.Session.Combat.ActivedPlanets.Contains(factory.planetId))
        {
            return true;
        }
        if (target.type == ETargetType.Player)
        {
            var playerId = target.id;
            var players = Multiplayer.Session.Combat.Players;
            for (var i = 0; i < players.Length; i++)
            {
                if (players[i].id == playerId)
                {
                    ref var ptr = ref players[i];
                    dist2 = Vector3.SqrMagnitude(ptr.skillTargetL - __instance.muzzleWPos);
                    __result = ptr.isAlive && (__instance.CounterAttackPlayer(factory, @base) || dist2 <= (__instance.sensorRange * __instance.sensorRange));
                    return false;
                }
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGTurretComponent.SearchTarget))]
    public static bool SearchTarget(ref DFGTurretComponent __instance, PlanetFactory factory, DFGBaseComponent @base,
        EAggressiveLevel aggressiveLevel, ref bool __result)
    {
        var planetId = factory.planetId;
        if (!Multiplayer.IsActive || !Multiplayer.Session.Combat.ActivedPlanets.Contains(planetId))
        {
            return true;
        }
        if (aggressiveLevel <= EAggressiveLevel.Passive)
        {
            return true;
        }

        // Find the closest alive player to the turret
        var playerId = -1;
        var cloestDist = float.MaxValue;
        var players = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < players.Length; i++)
        {
            ref var ptr = ref players[i];
            if (ptr.planetId != planetId || !ptr.isAlive)
            {
                continue;
            }
            var dist = Vector3.SqrMagnitude(ptr.skillTargetL - __instance.muzzleWPos);
            if (dist < cloestDist)
            {
                playerId = ptr.id;
                cloestDist = dist;
            }
        }
        if (playerId == -1)
        {
            // If there is no alive player, return to original function
            return true;
        }

        var counterAttackFlag = __instance.CounterAttackPlayer(factory, @base)
            && (__instance.target.type != ETargetType.Player || __instance.target.id != playerId);
        if (counterAttackFlag || cloestDist <= __instance.realAttactRange * __instance.realAttactRange)
        {
            __instance.target.type = ETargetType.Player;
            __instance.target.id = playerId;
            __instance.state = EDFTurretState.Aiming;
            var sqrMagnitude = __instance.localDir.sqrMagnitude;
            var scale = Mathf.Sqrt(cloestDist / sqrMagnitude);
            __instance.localDir.x *= scale;
            __instance.localDir.y *= scale;
            __instance.localDir.z *= scale;

            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGTurretComponent.Aim))]
    public static void Aim(ref DFGTurretComponent __instance, PlanetFactory factory)
    {
        if (!Multiplayer.IsActive || __instance.target.type != ETargetType.Player) return;

        var pool = Multiplayer.Session.Combat.Players;
        for (var i = 0; i < pool.Length; i++)
        {
            if (pool[i].id == __instance.target.id)
            {
                factory.skillSystem.playerSkillTargetL = pool[i].skillTargetL;
                return;
            }
        }
    }
}
