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
        if (Multiplayer.Session.IsClient) return false;

        // Server trigger active event and broadcast to clients
        var planetId = __instance.groundSystem.planet.id;
        var starId = __instance.groundSystem.planet.star.id;
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

                        // Broadcast the active unit event to clients
                        var packet = new DFGActivateUnitPacket(planetId, __instance.id,
                            formId, portId, EEnemyBehavior.KeepForm, 240, ptr[unitId].enemyId);
                        Multiplayer.Session.Network.SendPacketToStar(packet, starId);
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
                    ref var enemyUnit = ref __instance.groundSystem.units.buffer[unitId];
                    enemyUnit.behavior = EEnemyBehavior.KeepForm;
                    enemyUnit.stateTick = 120;

                    // Broadcast the active unit event to clients
                    var planetId = __instance.groundSystem.planet.id;
                    var starId = __instance.groundSystem.planet.star.id;
                    var packet = new DFGActivateUnitPacket(planetId, __instance.id,
                        formId, portId, EEnemyBehavior.KeepForm, 120, enemyUnit.enemyId);
                    Multiplayer.Session.Network.SendPacketToStar(packet, starId);
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
            var packet = new DFGActivateBasePacket(__instance.groundSystem.planet.id, __instance.id, true);
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
                            ref var enemyUnit = ref __instance.groundSystem.units.buffer[unitId];
                            enemyUnit.behavior = eenemyBehavior;
                            enemyUnit.stateTick = setStateTick;

                            // Broadcast the active unit event to clients
                            var planetId = __instance.groundSystem.planet.id;
                            var starId = __instance.groundSystem.planet.star.id;
                            var packet = new DFGActivateUnitPacket(planetId, __instance.id,
                                formId, portId, eenemyBehavior, setStateTick, enemyUnit.enemyId);
                            Multiplayer.Session.Network.SendPacketToStar(packet, starId);
                        }
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UpdateHatred))]
    public static bool UpdateHatred_Prefix(DFGBaseComponent __instance, long gameTick, int hatredTake, int maxDispatch)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;

        // PlayerAction_Combat.ActivateNearbyEnemyBase
        var planetId = __instance.groundSystem.planet.id;
        var enemyPool = __instance.groundSystem.factory.enemyPool;
        var players = Multiplayer.Session.Combat.Players;
        for (var pid = 0; pid < players.Length; pid++)
        {
            if (players[pid].planetId != planetId) continue;
            // Balance: Increase base player alert range from 90 to 200
            if (((Vector3)enemyPool[__instance.enemyId].pos - players[pid].position).sqrMagnitude < 40000.0)
            {
                __instance.UnderAttack(players[pid].position, 50f, 120);
            }
        }

        // Active units for each hatredTake until maxDispatch is reach
        if (__instance.hatred.max.value < hatredTake) return false;
        ref var unitBuffer = ref __instance.groundSystem.units.buffer;
        for (var formId = 0; formId < 3; formId++)
        {
            var portCount = __instance.forms[formId].portCount;
            for (var portId = 1; portId <= portCount; portId++)
            {
                if (__instance.forms[formId].units[portId] == 1)
                {
                    if (maxDispatch <= 0 || __instance.hatred.max.value < hatredTake)
                    {
                        return false;
                    }
                    var unitId = __instance.groundSystem.ActivateUnit(__instance.id, formId, portId, gameTick);
                    if (unitId > 0)
                    {
                        ref var enemyUnit = ref unitBuffer[unitId];
                        enemyUnit.hatred.HateTarget(__instance.hatred.max.objectType, __instance.hatred.max.objectId, hatredTake, hatredTake, EHatredOperation.Set);
                        enemyUnit.behavior = EEnemyBehavior.SeekForm;
                        enemyUnit.stateTick = 120;
                        __instance.hatred.max.value -= hatredTake;
                        __instance.hatred.Arrange();
                        maxDispatch--;

                        // Broadcast the active unit event to clients
                        var starId = planetId / 100;
                        var packet = new DFGActivateUnitPacket(planetId, __instance.id,
                            formId, portId, EEnemyBehavior.SeekForm, 120, enemyUnit.enemyId);
                        Multiplayer.Session.Network.SendPacketToStar(packet, starId);
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.UpdateFactoryThreat))]
    public static bool UpdateFactoryThreat_Prefix(DFGBaseComponent __instance, EAggressiveLevel agglv)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        //Client: Let threat update by DFGUpdateBaseStatusPacket
        if (agglv <= EAggressiveLevel.Passive)
        {
            return false;
        }
        //Set threat share to 0 so it doesn't modify threat of the base
        __instance.evolve.threatshr = 0;
        if (__instance.evolve.waveTicks <= 0)
        {
            __instance.evolve.waveAsmTicks = 0;
            return false;
        }
        if (__instance.hasAssaultingUnit)
        {
            __instance.evolve.waveTicks++;
            return false;
        }
        __instance.evolve.waveTicks = 0;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFGBaseComponent.LaunchAssault))]
    public static bool LaunchAssault_Prefix(DFGBaseComponent __instance, Vector3 tarPos, float expandRadius,
        int unitCount0, int unitCount1, int ap0, int ap1, int unitThreat)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Combat.IsIncomingRequest.Value;

        var packet = new DFGLaunchAssaultPacket(__instance, in tarPos, expandRadius, unitCount0, unitCount1, ap0, ap1, unitThreat);
        Multiplayer.Session.Server.SendPacket(packet);
        return true;
    }
}
