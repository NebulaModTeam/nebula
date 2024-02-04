#region

using System;
using HarmonyLib;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;
using UnityEngine;
#pragma warning disable CA1861

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DFGBaseComponent))]
internal class DFGBaseComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UnderAttack), new Type[] { })]
    public static bool UnderAttack_Prefix1()
    {
        // Handle in PlayerAction_Combat.ActivateBaseEnemyManually
        return !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.ActiveAllUnit))]
    public static bool ActiveAllUnit_Prefix(DFGBaseComponent __instance, long gameTick)
    {
        if (!Multiplayer.IsActive) return true;
        if (!Multiplayer.Session.Combat.IsIncomingRequest.Value) return false;

        // This will only trigger in event instead of every actived tick
        ref var ptr = ref __instance.groundSystem.units.buffer;
        var cursor = __instance.groundSystem.units.cursor;
        for (var i = 1; i < cursor; i++)
        {
            if (ptr[i].baseId == __instance.id)
            {
                var behavior = ptr[i].behavior;
                if (behavior != EEnemyBehavior.Initial && behavior != EEnemyBehavior.Defense)
                {
                    ptr[i].stateTick = 240; // active tick (3+1) * keyFrame 60
                }
            }
        }

        var formLength = __instance.forms.Length;
        for (var formId = 0; formId < formLength; formId++)
        {
            var enemyFormation = __instance.forms[formId];
            var portCount = enemyFormation.portCount;
            for (var portId = 1; portId <= portCount; portId++)
            {
                if (enemyFormation.units[portId] == 1)
                {
                    var unitId = __instance.groundSystem.ActivateUnit(__instance.id, formId, portId, gameTick);
                    if (unitId > 0)
                    {
                        ptr[unitId].behavior = EEnemyBehavior.KeepForm;
                        ptr[unitId].stateTick = 240; // active tick (3+1) * keyFrame 60
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UnderAttack), new Type[] { typeof(int) })]
    public static bool UnderAttack_Prefix2(DFGBaseComponent __instance, int formId)
    {
        if (!Multiplayer.IsActive || __instance.activeTick > 0) return true;
        if (Multiplayer.Session.IsClient) return false;

        var gameTick = GameMain.gameTick;
        var enemyFormation = __instance.forms[formId];
        for (var portId = 1; portId <= enemyFormation.portCount; portId++)
        {
            if (enemyFormation.units[portId] == 1)
            {
                var unitId = __instance.groundSystem.ActivateUnit(__instance.id, formId, portId, gameTick);
                if (unitId > 0)
                {
                    var buffer = __instance.groundSystem.units.buffer;
                    buffer[unitId].behavior = EEnemyBehavior.KeepForm;
                    buffer[unitId].stateTick = 120;

                    // Broadcast the active unit event to clients
                    var planetId = __instance.groundSystem.planet.id;
                    var starId = __instance.groundSystem.planet.star.id;
                    Multiplayer.Session.Network.SendPacketToStar(
                        new ActivateGroundUnitPacket(planetId, __instance.id, formId, portId, (byte)EEnemyBehavior.KeepForm, 120, unitId),
                        starId);
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UnderAttack), new Type[] { typeof(Vector3), typeof(float), typeof(int), typeof(bool) })]
    public static bool UnderAttack_Prefix3(DFGBaseComponent __instance, Vector3 center, float radius, int setStateTick, bool setToSeekForm)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;


        if (setToSeekForm) // Bombed by EMP, change all KeepForm unit to SeekForm
        {
            ref var buffer = ref __instance.groundSystem.units.buffer;
            var cursor = __instance.groundSystem.units.cursor;
            for (var i = 1; i < cursor; i++)
            {
                if (buffer[i].baseId == __instance.id && buffer[i].behavior == EEnemyBehavior.KeepForm)
                {
                    buffer[i].behavior = EEnemyBehavior.SeekForm;
                }
            }
            var packet = new ActivateBasePacket(__instance.groundSystem.planet.id, __instance.id, true);
            Multiplayer.Session.Network.SendPacketToStar(packet, __instance.groundSystem.planet.star.id);
        }
        if (__instance.activeTick > 0)
        {
            return false;
        }
        var enemyData = default(EnemyData);
        var planet = __instance.groundSystem.planet;
        var gameTick = GameMain.gameTick;
        var num = (int)((planet.seed + gameTick) % 151200L);
        ref var ptr = ref __instance.groundSystem.factory.enemyPool[__instance.enemyId];
        var realRadius = planet.realRadius;
        var pos = VectorLF3.zero;
        var rot = Quaternion.identity;
        var vel = Vector3.zero;
        var x = center.x;
        var y = center.y;
        var z = center.z;
        var radiusSqrt = radius * radius;
        var eenemyBehavior = (setToSeekForm ? EEnemyBehavior.SeekForm : EEnemyBehavior.KeepForm);
        for (var formId = 0; formId < __instance.forms.Length; formId++)
        {
            var enemyFormation = __instance.forms[formId];
            for (var portId = 1; portId <= enemyFormation.portCount; portId++)
            {
                if (enemyFormation.units[portId] == 1)
                {
                    enemyData.protoId = (short)(formId + 8128);
                    enemyData.owner = (short)__instance.id;
                    enemyData.port = (short)portId;
                    enemyData.Formation(num, ref ptr, realRadius, ref pos, ref rot, ref vel);
                    var dx = (float)pos.x - x;
                    var dy = (float)pos.y - y;
                    var dz = (float)pos.z - z;
                    if (dx * dx + dy * dy + dz * dz < radiusSqrt)
                    {
                        var unitId = __instance.groundSystem.ActivateUnit(__instance.id, formId, portId, gameTick);
                        if (unitId > 0)
                        {
                            var buffer = __instance.groundSystem.units.buffer;
                            buffer[unitId].behavior = eenemyBehavior;
                            buffer[unitId].stateTick = setStateTick;

                            // Broadcast the active unit event to clients
                            var planetId = __instance.groundSystem.planet.id;
                            var starId = __instance.groundSystem.planet.star.id;
                            Multiplayer.Session.Network.SendPacketToStar(
                                new ActivateGroundUnitPacket(planetId, __instance.id, formId, portId, (byte)eenemyBehavior, setStateTick, unitId),
                                starId);
                        }
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UpdateHatred))]
    public static bool UpdateHatred_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Note: Figure out hatred mechanism in the future
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.LaunchAssault))]
    public static bool LaunchAssault_Prefix(DFGBaseComponent __instance, Vector3 tarPos, float expandRadius,
        int unitCount0, int unitCount1, int ap0, int ap1, int unitThreat)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Combat.IsIncomingRequest.Value;

        var packet = new LaunchAssaultPacket(__instance, in tarPos, expandRadius, unitCount0, unitCount1, ap0, ap1, unitThreat);
        Multiplayer.Session.Network.SendPacketToStar(packet, __instance.groundSystem.planet.star.id);
        return true;
    }
}
