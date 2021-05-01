using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationWindow))]
    class UIStationWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMaxChargePowerSliderValueChange")]
        public static bool OnMaxChargePowerSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS)
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxChargePower, value);
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
        public static bool OnMaxTripDroneSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id,__instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxTripDrones, value);
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
        public static bool OnMaxTripVesselSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxTripVessel, value);
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
        public static bool OnMinDeliverDroneValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MinDeliverDrone, value);
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
        public static bool OnMinDeliverVesselValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MinDeliverVessel, value);
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
        public static bool OnWarperDistanceValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarpDistance, value);
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
        public static bool OnWarperNecessaryClick_Prefix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarperNeeded, 0f);
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
        public static bool OnIncludeOrbitCollectorClick_Prefix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS && (StationUIManager.UIIsSyncedStage == 2 || LocalPlayer.IsMasterClient))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.IncludeCollectors, 0f);
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
        public static bool OnDroneIconClick_Prefix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS)
            {
                Player player = GameMain.mainPlayer;
                if (player.inhandItemCount > 0 && player.inhandItemId != 5001)
                {
                    ItemProto itemProto = LDB.items.Select(5001);
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
                    return false;
                }
                StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                int toAdd;
                if (player.inhandItemCount > 0)
                {
                    int droneAmount = stationComponent.idleDroneCount + stationComponent.workDroneCount;
                    int spaceLeft = 50 - droneAmount;
                    if (spaceLeft < 0)
                    {
                        spaceLeft = 0;
                    }
                    toAdd = (__instance.player.inhandItemCount >= spaceLeft) ? spaceLeft : __instance.player.inhandItemCount;
                }
                else
                {
                    toAdd = stationComponent.idleDroneCount * -1;
                }
                if (!LocalPlayer.IsMasterClient)
                {
                    StationUIManager.UIRequestedShipDronWarpChange = true;
                }

                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + toAdd);
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
        public static bool OnShipIconClick_Prefix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS)
            {
                Player player = GameMain.mainPlayer;
                if(player.inhandItemCount > 0 && player.inhandItemId != 5002)
                {
                    ItemProto itemProto = LDB.items.Select(5002);
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
                    return false;
                }
                StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                int toAdd;
                if(player.inhandItemCount > 0)
                {
                    int shipAmount = stationComponent.idleShipCount + stationComponent.workShipCount;
                    int spaceLeft = 10 - shipAmount;
                    if (spaceLeft < 0)
                    {
                        spaceLeft = 0;
                    }
                    toAdd = (__instance.player.inhandItemCount >= spaceLeft) ? spaceLeft : __instance.player.inhandItemCount;
                }
                else
                {
                    toAdd = stationComponent.idleShipCount * -1;
                }
                if (!LocalPlayer.IsMasterClient)
                {
                    StationUIManager.UIRequestedShipDronWarpChange = true;
                }
                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + toAdd);
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
        public static bool OnWarperIconClick_Prefix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS)
            {
                Player player = GameMain.mainPlayer;
                if (player.inhandItemCount > 0 && player.inhandItemId != 1210)
                {
                    ItemProto itemProto = LDB.items.Select(1210);
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
                    return false;
                }
                StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                int toAdd;
                if (player.inhandItemCount > 0)
                {
                    int spaceLeft = 50 - stationComponent.warperCount;
                    if (spaceLeft < 0)
                    {
                        spaceLeft = 0;
                    }
                    toAdd = (__instance.player.inhandItemCount >= spaceLeft) ? spaceLeft : __instance.player.inhandItemCount;
                }
                else
                {
                    toAdd = stationComponent.warperCount * -1;
                }
                if (!LocalPlayer.IsMasterClient)
                {
                    StationUIManager.UIRequestedShipDronWarpChange = true;
                }

                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount + toAdd);
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
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || StationUIManager.UIIsSyncedStage > 0 || GameMain.localPlanet == null || !__instance.active)
            {
                return true;
            }
            ((Text)AccessTools.Field(typeof(UIStationWindow), "titleText").GetValue(__instance)).text = "Loading...";
            StationUIManager.LastSelectedGameObj = EventSystem.current.currentSelectedGameObject;
            if(__instance.factory == null)
            {
                __instance.factory = GameMain.localPlanet.factory;
            }
            if(__instance.transport == null)
            {
                __instance.transport = __instance.factory.transport;
            }
            StationComponent stationComponent = null;
            if(__instance.stationId == 0)
            {
                UIStationStorage[] stationStorage = (UIStationStorage[])AccessTools.Field(typeof(UIStationWindow), "storageUIs").GetValue(__instance);
                if(stationStorage != null && stationStorage[0] != null && stationStorage[0].station.id != 0)
                {
                    stationComponent = __instance.transport.stationPool[stationStorage[0].station.id];
                }
            }
            else
            {
                stationComponent = __instance.transport.stationPool[__instance.stationId];
            }
            if(stationComponent != null && GameMain.localPlanet != null)
            {
                int id = (stationComponent.isStellar == true) ? stationComponent.gid : stationComponent.id;
                // for some reason PLS has planetId set to 0, so we use players localPlanet here (he should be on a planet anyways when opening the UI)
                LocalPlayer.SendPacket(new StationUIInitialSyncRequest(stationComponent.planetId, stationComponent.id, stationComponent.gid));
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
                // it is actually 0 before we manually set it to the right value in StationUIInitialSyncProcessor.cs and thus its a good check to skip sending the packet on the Free() call
                LocalPlayer.SendPacket(new StationSubscribeUIUpdates(false, __instance.transport.planet.id, __instance.transport.stationPool[StationUIManager.UIStationId].id, __instance.transport.stationPool[StationUIManager.UIStationId].gid));
                StationUIManager.LastSelectedGameObj = null;
                StationUIManager.UIIsSyncedStage = 0;
                StationUIManager.UIStationId = 0;
            }
        }
    }
}
