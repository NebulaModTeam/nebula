using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using NebulaModel.Logger;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationWindow))]
    class UIStationWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMaxChargePowerSliderValueChange")]
        public static bool OnMaxChargePowerSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 0, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMaxTripDroneSliderValueChange")]
        public static bool OnMaxTripDroneSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 1, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMaxTripVesselSliderValueChange")]
        public static bool OnMaxTripVesselSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 2, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMinDeliverDroneValueChange")]
        public static bool OnMinDeliverDroneValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 3, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMinDeliverVesselValueChange")]
        public static bool OnMinDeliverVesselValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 4, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperDistanceValueChange")]
        public static bool OnWarperDistanceValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 5, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperNecessaryClick")]
        public static bool OnWarperNecessaryClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 6, 0f);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnIncludeOrbitCollectorClick")]
        public static bool OnIncludeOrbitCollectorClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"] && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 7, 0f);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDroneIconClick")]
        public static bool OnDroneIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 8, __instance.transport.stationPool[__instance.stationId].idleDroneCount);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnShipIconClick")]
        public static bool OnShipIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 9, __instance.transport.stationPool[__instance.stationId].idleShipCount);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperIconClick")]
        public static bool OnWarperIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 10, __instance.transport.stationPool[__instance.stationId].warperCount);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnStationIdChange")]
        public static bool OnStationIdChange_Prefix(UIStationWindow __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || StationUIManager.UIIsSyncedStage > 0 || GameMain.localPlanet == null)
            {
                return true;
            }
            Debug.Log("loading...");
            ((Text)AccessTools.Field(typeof(UIStationWindow), "titleText").GetValue(__instance)).text = "Loading...";
            StationUIManager.lastSelectedGameObj = EventSystem.current.currentSelectedGameObject;
            if(__instance.factory == null)
            {
                __instance.factory = GameMain.localPlanet.factory;
            }
            if(__instance.transport == null)
            {
                __instance.transport = __instance.factory.transport;
            }
            if(__instance.stationId == 0)
            {
                UIStationStorage[] stationStorage = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(__instance);
                if(stationStorage != null && stationStorage[0] != null && stationStorage[0].station.id != 0)
                {
                    Log.Info($"sending initial sync request id {__instance.transport.stationPool[stationStorage[0].station.id]} size: {__instance.transport.stationPool.Length}");
                    Log.Info($"gid: {__instance.transport.stationPool[stationStorage[0].station.id].gid}");
                    Debug.Log((int)AccessTools.Field(typeof(UIStationWindow), "_stationId").GetValue(__instance));
                    LocalPlayer.SendPacket(new StationUIInitialSyncRequest(__instance.transport.stationPool[stationStorage[0].station.id].gid));
                    StationUIManager.UIIsSyncedStage++;
                }
                else
                {
                    Debug.Log("cant proceed :C");
                }
            }
            else
            {
                Debug.Log("sending initial sync request " + __instance.transport.stationPool[__instance.stationId].gid);
                Debug.Log((int)AccessTools.Field(typeof(UIStationWindow), "_stationId").GetValue(__instance));
                Debug.Log("gStationCursorl: " + GameMain.data.galacticTransport.stationCursor + " Len: " + GameMain.data.galacticTransport.stationPool.Length);
                LocalPlayer.SendPacket(new StationUIInitialSyncRequest(__instance.transport.stationPool[__instance.stationId].gid));
                StationUIManager.UIIsSyncedStage++;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("_OnUpdate")]
        public static bool _OnUpdate_Prefix(UIStationWindow __instance)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || StationUIManager.UIIsSyncedStage == 2)
            {
                return true;
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void _OnClose_Postfix(UIStationWindow __instance)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }
            if (__instance.factory == null)
            {
                __instance.factory = GameMain.localPlanet.factory;
            }
            if (__instance.transport == null)
            {
                __instance.transport = __instance.factory.transport;
            }
            if(__instance.stationId != 0 || StationUIManager.UIStationId != 0)
            {
                Debug.Log("sending unsubscriber");
                // it is actually 0 before we manually set it to the right value in StationUIInitialSyncProcessor.cs and thus its a good check to skip sending the packet on the Free() call
                LocalPlayer.SendPacket(new StationSubscribeUIUpdates(false, __instance.transport.stationPool[StationUIManager.UIStationId].gid));
                StationUIManager.lastSelectedGameObj = null;
                StationUIManager.UIIsSyncedStage = 0;
                StationUIManager.UIStationId = 0;
            }
            else
            {
                Debug.Log("skiping unsubscribe");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Postfix()
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || StationUIManager.UIIsSyncedStage > -1)
            {
                return;
            }
            //Debug.Log("increase");
            //StationUIManager.UIIsSyncedStage++;
        }
    }
}
