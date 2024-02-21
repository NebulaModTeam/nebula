#region

using HarmonyLib;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyDFHiveSystem))]
internal class EnemyDFHiveSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ActiveAllUnit))]
    public static bool ActiveAllUnit_Prefix(EnemyDFHiveSystem __instance, long gameTick)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;

        if (!__instance.realized)
        {
            return false;
        }
        var cursor = __instance.units.cursor;
        for (var unitId = 1; unitId < cursor; unitId++)
        {
            if (__instance.units.buffer[unitId].behavior == EEnemyBehavior.KeepForm)
            {
                __instance.units.buffer[unitId].stateTick = 600;
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
                    var unitId = __instance.ActivateUnit(formId, portId, gameTick);
                    if (unitId > 0)
                    {
                        var buffer = __instance.units.buffer;
                        buffer[unitId].behavior = EEnemyBehavior.KeepForm;
                        buffer[unitId].stateTick = 600;
                        var packet = new DFSActivateUnitPacket(__instance.hiveAstroId, formId, portId,
                            (byte)EEnemyBehavior.KeepForm, 600, unitId, buffer[unitId].enemyId);
                        Multiplayer.Session.Server.SendPacket(packet);
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.AssaultingWavesDetermineAI))]
    public static bool AssaultingWavesDetermineAI_Prefix(EnemyDFHiveSystem __instance, EAggressiveLevel aggressiveLevel)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        if (aggressiveLevel <= EAggressiveLevel.Passive)
        {
            return false;
        }
        if (__instance.lancerAssaultCountBase < 1f || __instance.lancerAssaultCountBase > 1500f)
        {
            __instance.lancerAssaultCountBase = __instance.GetLancerAssaultCountInitial(aggressiveLevel);
        }
        if (__instance.lancerAssaultCountBase > 360f)
        {
            __instance.lancerAssaultCountBase = 360f;
        }
        // Skip the part of if (this.evolve.threat >= this.evolve.maxThreat) in client
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.DeactivateUnit))]
    public static bool DeactivateUnit_Prefix(EnemyDFHiveSystem __instance, int unitId)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        var enemyId = __instance.units.buffer[unitId].enemyId;
        if (enemyId == 0)
        {
            return false;
        }
        ref var ptr = ref __instance.sector.enemyPool[enemyId];
        if (ptr.id != 0 && ptr.id == enemyId)
        {
            var port = (int)ptr.port;
            var formId = 8113 - ptr.protoId;
            if (__instance.hiveAstroId != ptr.originAstroId) // Assert.True(__instance.hiveAstroId == ptr.originAstroId)
            {
                return false;
            }
            var enemyFormation = __instance.forms[formId];
            if (enemyFormation.units[port] > 1) // Assert.True(enemyFormation.units[port] > 1
            {
                enemyFormation.units[port] = 1;
            }
            __instance.sector.RemoveEnemyFinal(enemyId);
            Multiplayer.Session.Network.SendPacket(new DFSDeactivateUnitPacket(__instance.hiveAstroId, enemyId));
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ExecuteDeferredEnemyChange))]
    public static bool ExecuteDeferredEnemyChange_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient)
        {
            __instance._add_relay_list?.Clear();
            __instance._add_tinder_list?.Clear();
            __instance._rmv_id_list?.Clear();
            __instance._add_bidx_list?.Clear();
            return false;
        }

        var hiveAstroId = __instance.hiveAstroId;
        if (__instance._add_relay_list != null && __instance._add_relay_list.Count > 0)
        {
            foreach (var dockIndex in __instance._add_relay_list)
            {
                ref var ptr = ref __instance.relayDocks[dockIndex % __instance.relayDocks.Length];
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, 8116, __instance.hiveAstroId, ptr.pos, ptr.rot);
                var dfRelayId = __instance.sector.enemyPool[enemyId].dfRelayId;
                var dfrelayComponent = __instance.relays.buffer[dfRelayId];
                if (dfrelayComponent != null)
                {
                    dfrelayComponent.SetDockIndex(dockIndex);
                    __instance.AddIdleRelay(dfRelayId);
                }
                Multiplayer.Session.Network.SendPacket(new DFSAddIdleRelayPacket(hiveAstroId, dockIndex, enemyId));
            }
            __instance._add_relay_list.Clear();
        }
        if (__instance._add_tinder_list != null && __instance._add_tinder_list.Count > 0)
        {
            foreach (var dockIndex in __instance._add_tinder_list)
            {
                ref var ptr2 = ref __instance.tinderDocks[dockIndex % __instance.tinderDocks.Length];
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, 8119, __instance.hiveAstroId, ptr2.pos, ptr2.rot);
                var dfTinderId = __instance.sector.enemyPool[enemyId].dfTinderId;
                ref var tinder = ref __instance.tinders.buffer[dfTinderId];
                if (dfTinderId > 0)
                {
                    tinder.SetDockIndex(__instance, dockIndex);
                    __instance.AddIdleTinder(dfTinderId);
                }
                Multiplayer.Session.Network.SendPacket(new DFSAddIdleTinderPacket(hiveAstroId, dockIndex, enemyId));
            }
            __instance._add_tinder_list.Clear();
            __instance._add_tinder_list = null;
        }
        if (!__instance.realized)
        {
            return false;
        }
        if (__instance._rmv_id_list != null && __instance._rmv_id_list.Count > 0)
        {
            foreach (var enemyId in __instance._rmv_id_list)
            {
                __instance.sector.RemoveEnemyFinal(enemyId);
                Multiplayer.Session.Network.SendPacket(new DFSRemoveEnemyDeferredPacket(enemyId));
            }
            __instance._rmv_id_list.Clear();
        }
        if (__instance._add_bidx_list != null && __instance._add_bidx_list.Count > 0)
        {
            foreach (var builderIndex in __instance._add_bidx_list)
            {
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, builderIndex, false);
                Multiplayer.Session.Network.SendPacket(new DFSAddEnemyDeferredPacket(hiveAstroId, builderIndex, enemyId));
            }
            __instance._add_bidx_list.Clear();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ExecuteDeferredUnitFormation))]
    public static bool ExecuteDeferredUnitFormation_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient)
        {
            __instance._initiate_unit_list?.Clear();
            __instance._add_tinder_list?.Clear();
            return false;
        }

        __instance._initiate_unit_list?.Clear();
        if (__instance._deactivate_unit_list != null && __instance._deactivate_unit_list.Count > 0)
        {
            foreach (var unitId in __instance._deactivate_unit_list)
            {
                __instance.DeactivateUnit(unitId);
            }
            __instance._deactivate_unit_list.Clear();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.LaunchLancerAssault))]
    public static bool LaunchLancerAssault_Prefix(EnemyDFHiveSystem __instance, EAggressiveLevel aggressiveLevel, 
        Vector3 tarPos, Vector3 maxHatredPos, int targetAstroId, int unitCount0, int unitThreat)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        // Brocast launch assault events to all players
        var packet = new DFSLaunchLancerAssaultPacket(in __instance, aggressiveLevel,
            in tarPos, in maxHatredPos, targetAstroId, unitCount0, unitThreat);
        Multiplayer.Session.Server.SendPacket(packet);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.NotifyRelayKilled))]
    public static bool NotifyRelayKilled(EnemyDFHiveSystem __instance, ref EnemyData enemy)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        var dfrelayComponent = __instance.relays.buffer[enemy.dfRelayId];
        if (dfrelayComponent != null && dfrelayComponent.id == enemy.dfRelayId)
        {
            __instance.relayNeutralizedCounter++;
            if (dfrelayComponent.baseState == 1 && dfrelayComponent.stage == 2)
            {
                var planetData = __instance.sector.galaxy.PlanetById(dfrelayComponent.targetAstroId);
                if (planetData != null)
                {
                    //Don't call GetOrCreateFactory in client. Only realize if the factory is already load from server
                    if (planetData.factory != null)
                    {
                        dfrelayComponent.RealizePlanetBase(__instance.sector);
                    }
                }
            }
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.Realize))]
    public static bool Realize_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        if (!__instance.realized)
        {
            Multiplayer.Session.Network.SendPacket(new DFHiveRealizePacket(__instance.hiveAstroId));
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.OpenPreview))]
    public static bool OpenPreview_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (!Multiplayer.Session.Enemies.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DFHivePreviewPacket(__instance.hiveAstroId, true));
        }
        return Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ClosePreview))]
    public static bool ClosePreview_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (!Multiplayer.Session.Enemies.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DFHivePreviewPacket(__instance.hiveAstroId, false));
        }
        return Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.KillCorruptedUnits))]
    public static bool KillCorruptedUnits_Prefix()
    {
        // Don't run enemy position check on client
        return !Multiplayer.IsActive || Multiplayer.Session.IsServer;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.UpdateHatred))]
    public static bool UpdateHatred_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Note: Figure out hatred mechanism in the future
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.UnderAttack),
        [typeof(VectorLF3), typeof(float)], [ArgumentType.Ref, ArgumentType.Normal])]
    public static bool UnderAttack_Prefix(EnemyDFHiveSystem __instance, ref VectorLF3 centerUPos, float radius)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return false;

        if (!__instance.realized)
        {
            return false;
        }
        __instance.sector.InverseTransformToAstro_ref(__instance.hiveAstroId, ref centerUPos, out var localPos);
        var enemyData = default(EnemyData);
        var formTicks = (int)((__instance.starData.seed + GameMain.gameTick) % 1512000L);
        var pos = VectorLF3.zero;
        var vel = Vector3.zero;
        var rot = Quaternion.identity;
        for (var formId = 0; formId < __instance.forms.Length; formId++)
        {
            var enemyFormation = __instance.forms[formId];
            for (var portId = 1; portId <= enemyFormation.portCount; portId++)
            {
                if (enemyFormation.units[portId] == 1)
                {
                    enemyData.protoId = (short)(8113 - formId);
                    enemyData.port = (short)portId;
                    enemyData.owner = (short)(__instance.hiveAstroId - 1000000);
                    enemyData.Formation(formTicks, (float)__instance.orbitRadius, ref pos, ref rot, ref vel);
                    var dx = localPos.x - pos.x;
                    var dy = localPos.y - pos.y;
                    var dz = localPos.z - pos.z;
                    if (dx * dx + dy * dy + dz * dz <= (double)(radius * radius))
                    {
                        var unitId = __instance.ActivateUnit(formId, portId, GameMain.gameTick);
                        if (unitId > 0)
                        {
                            var buffer = __instance.units.buffer;
                            buffer[unitId].behavior = EEnemyBehavior.KeepForm;
                            buffer[unitId].stateTick = 600;
                            var packet = new DFSActivateUnitPacket(__instance.hiveAstroId, formId, portId,
                                (byte)EEnemyBehavior.KeepForm, 600, unitId, buffer[unitId].enemyId);
                            Multiplayer.Session.Server.SendPacket(packet);
                        }
                    }
                }
            }
        }
        return false;
    }
}
