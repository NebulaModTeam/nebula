using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
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
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxChargePower, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnMaxTripDroneSliderValueChange))]
        public static bool OnMaxTripDroneSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxTripDrones, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnMaxTripVesselSliderValueChange))]
        public static bool OnMaxTripVesselSliderValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MaxTripVessel, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverDroneValueChange))]
        public static bool OnMinDeliverDroneValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MinDeliverDrone, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverVesselValueChange))]
        public static bool OnMinDeliverVesselValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.MinDeliverVessel, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnWarperDistanceValueChange))]
        public static bool OnWarperDistanceValueChange_Prefix(UIStationWindow __instance, float value)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarpDistance, value);
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
        [HarmonyPatch(nameof(UIStationWindow.OnWarperNecessaryClick))]
        public static bool OnWarperNecessaryClick_Prefix(UIStationWindow __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarperNeeded, 0f);
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
        [HarmonyPatch(nameof(UIStationWindow.OnIncludeOrbitCollectorClick))]
        public static bool OnIncludeOrbitCollectorClick_Prefix(UIStationWindow __instance)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS && (Multiplayer.Session.StationsUI.UIIsSyncedStage == 2 || Multiplayer.Session.LocalPlayer.IsHost))
            {
                StationUI packet = new StationUI(__instance.factory.planet.id, __instance.factory.transport.stationPool[__instance.stationId].id, __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.IncludeCollectors, 0f);
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
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.StationsUI.UIIsSyncedStage > 0)
            {
                return;
            }

            // Hide UI elements until sync data arrive
            __instance.titleText.text = "Loading...";
            for (int i = 0; i < __instance.storageUIs.Length; i++)
            {
                __instance.storageUIs[i]._Close();
                __instance.storageUIs[i].ClosePopMenu();
            }
            __instance.panelDown.SetActive(false);

            StationComponent stationComponent = __instance.transport?.stationPool[__instance.stationId];
            if (stationComponent != null && __instance.factory != null)
            {
                // for some reason advance miner has planetId set to 0, so we use UI's factory planetId
                Multiplayer.Session.Network.SendPacket(new StationUIInitialSyncRequest(__instance.factory.planetId, stationComponent.id, stationComponent.gid));
                Multiplayer.Session.StationsUI.UIIsSyncedStage++;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationWindow._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _OnUpdate_Prefix()
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.StationsUI.UIIsSyncedStage == 2)
            {
                return true;
            }
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStationWindow._OnClose))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnClose_Postfix(UIStationWindow __instance)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return;
            }
            if (__instance.stationId != 0)
            {
                Multiplayer.Session.StationsUI.UIIsSyncedStage = 0;
            }
        }
    }
}
