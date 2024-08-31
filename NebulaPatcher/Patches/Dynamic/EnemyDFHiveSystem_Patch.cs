#region

using System;
using HarmonyLib;
using NebulaAPI.DataStructures;
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
        if (__instance._add_tinder_list?.Count > 0)
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
        if (__instance._rmv_id_list?.Count > 0)
        {
            foreach (var enemyId in __instance._rmv_id_list)
            {
                __instance.sector.RemoveEnemyFinal(enemyId);
                Multiplayer.Session.Network.SendPacket(new DFSRemoveEnemyDeferredPacket(enemyId));
            }
            __instance._rmv_id_list.Clear();
        }
        if (__instance._add_bidx_list?.Count > 0)
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
        if (__instance._deactivate_unit_list?.Count > 0)
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
    [HarmonyPatch(nameof(EnemyDFHiveSystem.GameTickLogic))]
    public static void GameTickLogic_Prefix(EnemyDFHiveSystem __instance, long gameTick)
    {
        if (!Multiplayer.IsActive) return;

        if (Multiplayer.Session.IsServer)
        {
            // Broadcast hive level changes before adding units
            Multiplayer.Session.Enemies.BroadcastHiveStatusPackets(__instance, gameTick);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.LaunchLancerAssault))]
    public static bool LaunchLancerAssault_Prefix(EnemyDFHiveSystem __instance, EAggressiveLevel aggressiveLevel,
        Vector3 tarPos, Vector3 maxHatredPos, int targetAstroId, int unitCount0, int unitThreat)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        // Broadcast launch assault events to all players
        var packet = new DFSLaunchLancerAssaultPacket(in __instance, aggressiveLevel,
            in tarPos, in maxHatredPos, targetAstroId, unitCount0, unitThreat);
        Multiplayer.Session.Server.SendPacket(packet);
        Multiplayer.Session.Enemies.SendAstroMessage("Space hive is attacking".Translate(), packet.TargetAstroId);
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.NotifyRelayKilled))]
    public static bool NotifyRelayKilled_Prefix(EnemyDFHiveSystem __instance, ref EnemyData enemy)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsServer)
        {
            Multiplayer.Session.Enemies.SendAstroMessage("DF relay killed on".Translate(), enemy.astroId);
            return true;
        }

        var dfrelayComponent = __instance.relays.buffer[enemy.dfRelayId];
        if (dfrelayComponent?.id == enemy.dfRelayId)
        {
            __instance.relayNeutralizedCounter++;
            // Client will wait for server to send RealizePlanetBase packet
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

        if (!__instance.realized && !__instance.isEmpty)
        {
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Server.SendPacket(new DFHiveOpenPreviewPacket(__instance, true));
                __instance.InstantiateEnemies();
            }
            else if (!Multiplayer.Session.Enemies.IsIncomingRequest)
            {
                Multiplayer.Session.Client.SendPacket(new DFHiveOpenPreviewPacket(__instance, false));
            }
            else
            {
                __instance.InstantiateEnemies();
            }
        }
        __instance.isPreview = true;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ClosePreview))]
    public static bool ClosePreview_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (!__instance.realized)
        {
            if (Multiplayer.Session.IsServer)
            {
                Multiplayer.Session.Server.SendPacket(new DFHiveClosePreviewPacket(__instance));
                __instance.UninstantiateEnemies();
            }
            else if (!Multiplayer.Session.Enemies.IsIncomingRequest)
            {
                Multiplayer.Session.Client.SendPacket(new DFHiveClosePreviewPacket(__instance));
            }
            else
            {
                __instance.UninstantiateEnemies();
            }
        }
        __instance.isPreview = false;
        return false;
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.SensorLogic))]
    public static bool SensorLogic_Prefix(EnemyDFHiveSystem __instance, EAggressiveLevel aggressiveLevel)
    {
        if (!Multiplayer.IsActive) return true;

        var flag = false;
        float num;

        switch (aggressiveLevel)
        {
            case EAggressiveLevel.Rampage: num = 1.25f; break;
            case EAggressiveLevel.Sharp: num = 1.1f; break;
            case EAggressiveLevel.Normal: num = 1.0f; break;
            case EAggressiveLevel.Torpid: num = 0.8f; break;
            default: return false;
        }

        var num2 = num * 16000f * (__instance.evolve.rank / 127f);
        var num3 = num * 6000f;
        var uPos = __instance.sector.astros[__instance.hiveAstroId - 1000000].uPos;

        // Use ActivedStars instead of this.local_player_exist_alive for multiple mecha
        var starId = __instance.starData.id;
        var localPlayerExist = Multiplayer.Session.Combat.ActivedStars.Contains(starId);
        if (localPlayerExist && aggressiveLevel > EAggressiveLevel.Passive)
        {
            var players = Multiplayer.Session.Combat.Players;
            for (var i = 0; i < players.Length; i++)
            {
                if (players[i].starId != starId || !players[i].isAlive) continue;

                // Test for player that is alive and in the same system
                var vectorLF = players[i].uPosition - __instance.starData.uPosition;
                var num4 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
                var num5 = __instance.orbitRadius - num4;
                if (num5 < 0.0)
                {
                    num5 = -num5;
                }
                if (num5 < (double)num3)
                {
                    var vectorLF2 = vectorLF * (__instance.orbitRadius / num4) + __instance.starData.uPosition;
                    var num6 = vectorLF2.x - uPos.x;
                    var num7 = vectorLF2.y - uPos.y;
                    var num8 = vectorLF2.z - uPos.z;
                    var num9 = Math.Sqrt(num6 * num6 + num7 * num7 + num8 * num8);
                    var num10 = ((__instance.hatred.max.targetType == ETargetType.Player) ? 1f : 0.8f);
                    if (num9 < (double)(num2 * num10))
                    {
                        var num11 = Maths.Clamp01(((double)num2 - num9) / (double)(num2 - 5500f)) * 15.0 + 3.0;
                        var num12 = Maths.Clamp01(((double)num3 - num5) / (double)(num3 - 1500f));
                        var num13 = __instance.sector.skillSystem.maxHatredSpaceTmp * num12 * num11;
                        // The player is close enough, add hatred
                        __instance.hatred.HateTarget(ETargetType.Player, players[i].id,
                            (int)(__instance.sector.skillSystem.enemyAggressiveHatredCoefTmp * num13),
                            __instance.sector.skillSystem.maxHatredSpaceHiveTmp / 10, EHatredOperation.Add);
                        flag = true;
                    }
                }
            }

            var craftPool = __instance.sector.craftPool;
            var craftCursor = __instance.sector.craftCursor;
            for (var i = 1; i < craftCursor; i++)
            {
                ref var ptr = ref craftPool[i];
                if (ptr.id == i && ptr.astroId == __instance.starData.astroId && !ptr.isInvincible)
                {
                    __instance.sector.TransformFromAstro_ref(ptr.astroId, out var upos, ref ptr.pos);
                    var vectorLF = upos - __instance.starData.uPosition;
                    var num14 = Math.Sqrt(vectorLF.x * vectorLF.x + vectorLF.y * vectorLF.y + vectorLF.z * vectorLF.z);
                    var num15 = __instance.orbitRadius - num14;
                    if (num15 < 0.0)
                    {
                        num15 = -num15;
                    }
                    if (num15 <= (double)num3)
                    {
                        var magnitude = (vectorLF * (__instance.orbitRadius / num14) + __instance.starData.uPosition - uPos).magnitude;
                        if (magnitude < (double)num2)
                        {
                            var num16 = Maths.Clamp01(((double)num2 - magnitude) / (double)(num2 - 5500f)) * 15.0 + 3.0;
                            var num17 = Maths.Clamp01(((double)num3 - num15) / (double)(num3 - 1500f));
                            var num18 = __instance.sector.skillSystem.maxHatredSpaceTmp * num17 * num16 * 0.6;
                            __instance.hatred.HateTarget(ETargetType.Craft, i,
                                (int)(__instance.sector.skillSystem.enemyAggressiveHatredCoefTmp * num18),
                                __instance.sector.skillSystem.maxHatredSpaceHiveTmp / 10, EHatredOperation.Add);
                            flag = true;
                        }
                    }
                }
            }
        }

        var model = __instance.sector.model;
        if (model != null)
        {
            if (__instance.isLocal && flag)
            {
                model.aggressiveHiveAstroId = __instance.hiveAstroId;
            }
            else if (model.aggressiveHiveAstroId == __instance.hiveAstroId)
            {
                model.aggressiveHiveAstroId = 0;
            }
        }
        return false;
    }
}
