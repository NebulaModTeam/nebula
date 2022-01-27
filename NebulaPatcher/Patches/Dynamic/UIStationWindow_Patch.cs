using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationWindow))]
    internal class UIStationWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnMaxChargePowerSliderValueChange))]
        public static bool OnMaxChargePowerSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxChargePower;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnMaxTripDroneSliderValueChange))]
        public static bool OnMaxTripDroneSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxTripDrones;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnMaxTripVesselSliderValueChange))]
        public static bool OnMaxTripVesselSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxTripVessel;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverDroneValueChange))]
        public static bool OnMinDeliverDroneValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverDrone;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverVesselValueChange))]
        public static bool OnMinDeliverVesselValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverVessel;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnWarperDistanceValueChange))]
        public static bool OnWarperDistanceValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.WarpDistance;
            Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnWarperNecessaryClick))]
        public static bool OnWarperNecessaryClick_Prefix(UIStationWindow __instance)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarperNeeded, 0f);
            Multiplayer.Session.Network.SendPacket(packet);
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnIncludeOrbitCollectorClick))]
        public static bool OnIncludeOrbitCollectorClick_Prefix(UIStationWindow __instance)
        {
            if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
            {
                return true;
            }
            StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.IncludeCollectors, 0f);
            Multiplayer.Session.Network.SendPacket(packet);
            return Multiplayer.Session.LocalPlayer.IsHost;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnDroneIconClick))]
        public static bool OnDroneIconClick_Prefix(UIStationWindow __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                Player player = GameMain.mainPlayer;
                if (player.inhandItemCount > 0 && player.inhandItemId != 5001)
                {
                    ItemProto itemProto = LDB.items.Select(5001);
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
                    return false;
                }
                StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                ItemProto stationItem = LDB.items.Select(__instance.factory.entityPool[stationComponent.entityId].protoId);
                
                int toAdd;
                if (player.inhandItemCount > 0)
                {
                    int droneAmount = stationComponent.idleDroneCount + stationComponent.workDroneCount;
                    int spaceLeft = stationItem.prefabDesc.stationMaxDroneCount - droneAmount;
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
                if (!Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.StationsUI.UIRequestedShipDronWarpChange = true;
                }

                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetDroneCount, stationComponent.idleDroneCount + toAdd);
                Multiplayer.Session.Network.SendPacket(packet);
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnShipIconClick))]
        public static bool OnShipIconClick_Prefix(UIStationWindow __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                Player player = GameMain.mainPlayer;
                if (player.inhandItemCount > 0 && player.inhandItemId != 5002)
                {
                    ItemProto itemProto = LDB.items.Select(5002);
                    UIRealtimeTip.Popup("只能放入".Translate() + itemProto.name, true, 0);
                    return false;
                }
                StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
                ItemProto stationItem = LDB.items.Select(__instance.factory.entityPool[stationComponent.entityId].protoId);
                
                int toAdd;
                if (player.inhandItemCount > 0)
                {
                    int shipAmount = stationComponent.idleShipCount + stationComponent.workShipCount;
                    int spaceLeft = stationItem.prefabDesc.stationMaxShipCount - shipAmount;
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
                if (!Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.StationsUI.UIRequestedShipDronWarpChange = true;
                }
                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetShipCount, stationComponent.idleShipCount + toAdd);
                Multiplayer.Session.Network.SendPacket(packet);

                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow.OnWarperIconClick))]
        public static bool OnWarperIconClick_Prefix(UIStationWindow __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
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
                    int spaceLeft = stationComponent.warperMaxCount - stationComponent.warperCount;
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
                if (!Multiplayer.Session.LocalPlayer.IsHost)
                {
                    Multiplayer.Session.StationsUI.UIRequestedShipDronWarpChange = true;
                }

                StationUI packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.SetWarperCount, stationComponent.warperCount + toAdd);
                Multiplayer.Session.Network.SendPacket(packet);
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStationWindow._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix(UIStationWindow __instance)
        {
            if (!Multiplayer.IsActive || __instance.transport == null)
            {
                return;
            }

            StationComponent stationComponent = __instance.transport.stationPool[__instance.stationId];
            Multiplayer.Session.StationsUI.SliderBarPacket = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid, StationUI.EUISettings.None, 0);
            Multiplayer.Session.StationsUI.StorageMaxChangeId = -1;
            if (Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }

            // Stage 0 : Hide UI elements until sync data arrive
            __instance.titleText.text = "Loading...";
            for (int i = 0; i < __instance.storageUIs.Length; i++)
            {
                __instance.storageUIs[i]._Close();
                __instance.storageUIs[i].ClosePopMenu();
            }
            __instance.panelDown.SetActive(false);

            // for some reason advance miner has planetId set to 0, so we use UI's factory planetId
            Multiplayer.Session.Network.SendPacket(new StationUIInitialSyncRequest(__instance.factory.planetId, stationComponent.id, stationComponent.gid));
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _OnUpdate_Prefix(UIStationWindow __instance)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }

            // When releasing left mouse button
            if (Input.GetMouseButtonUp(0))
            {
                if (Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex != StationUI.EUISettings.None)
                {
                    // Send SliderBarPacket when left mouse button is released
                    Multiplayer.Session.Network.SendPacket(Multiplayer.Session.StationsUI.SliderBarPacket);
                    Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.None;
                }
                if (Multiplayer.Session.StationsUI.StorageMaxChangeId >= 0)
                {
                    // Do the job in UIStationStorage.OnMaxSliderValueChange()
                    int index = Multiplayer.Session.StationsUI.StorageMaxChangeId;
                    float val = __instance.storageUIs[index].maxSlider.value;
                    StationStore stationStore = __instance.transport.stationPool[__instance.stationId].storage[index];
                    __instance.transport.SetStationStorage(__instance.stationId, index, stationStore.itemId, (int)(val * 100f + 0.5f), stationStore.localLogic, stationStore.remoteLogic, GameMain.mainPlayer);

                    // In client side, preserve displaying slider value until host response
                    Multiplayer.Session.StationsUI.StorageMaxChangeId = Multiplayer.Session.LocalPlayer.IsHost ? -1 : -2;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStationWindow._OnClose))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnClose_Postfix(UIStationWindow __instance)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
        }
    }
}
