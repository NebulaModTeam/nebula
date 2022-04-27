﻿using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Factory.Monitor;
using NebulaWorld.Warning;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
    [HarmonyPatch(typeof(MonitorComponent))]
    internal class MonitorComponent_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetPassColorId))]
        public static void SetPassColorId_Prefix(MonitorComponent __instance, byte __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                //Assume the monitor is on local planet
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetPassColorId, __0));                
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetFailColorId))]
        public static void SetFailColorId_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetFailColorId, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetPassOperator))]
        public static void SetPassOperator_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetPassOperator, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetMonitorMode))]
        public static void SetMonitorMode_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetMonitorMode, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetSystemWarningMode))]
        public static void SetSystemWarningMode_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetSystemWarningMode, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetSystemWarningSignalId))]
        public static void SetSystemWarningSignalId_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetSystemWarningSignalId, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetCargoFilter))]
        public static void SetCargoFilter_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetCargoFilter, __0));
            }
        }        

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetTargetCargoBytes))]
        public static void SSetTargetCargoBytes_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetTargetCargoBytes, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetPeriodTickCount))]
        public static void SetPeriodTickCount_Prefix(MonitorComponent __instance, int __0)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetPeriodTickCount, __0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.SetTargetBelt))]
        public static void SetTargetBelt_Prefix(MonitorComponent __instance, int __0, int __1)
        {
            //This is required for putting monitor over belt works properly
            if (Multiplayer.IsActive && !Multiplayer.Session.Warning.IsIncomingMonitorPacket)
            {
                int planetId = GameMain.data.localPlanet == null ? -1 : GameMain.data.localPlanet.id;
                Multiplayer.Session.Network.SendPacketToLocalStar(
                    new MonitorSettingUpdatePacket(planetId, __instance.id, MonitorSettingEvent.SetTargetBelt, __0, __1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MonitorComponent.InternalUpdate))]
        public static bool InternalUpdate_Prefix(MonitorComponent __instance, CargoTraffic _traffic, EntityData[] _entityPool, SpeakerComponent[] _speakerPool, AnimData[] _animPool)
        {
            if (Multiplayer.IsActive && __instance.targetBeltId > _traffic.beltPool.Length)
            {
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    _traffic.factory.RemoveEntityWithComponents(__instance.entityId);
                    WarningManager.DisplayTemporaryWarning($"Broken Traffic Monitor detected on {_traffic.factory.planet.displayName}\nIt was removed, clients should reconnect!", 15000);
                }
                return false;
            }
            return true;
        }
    }
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified
}
