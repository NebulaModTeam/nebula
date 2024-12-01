#region

using System;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelStationInspector))]
internal class UIControlPanelStationInspector_Patch
{
    private static long lastUpdateGametick;
    private static StationUI SliderBarPacket = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMaxChargePowerSliderValueChange))]
    public static bool OnMaxChargePowerSliderValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxChargePower;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        StringBuilderUtility.WriteKMG(__instance.powerServedSB, 8, (long)(3000000.0 * value + 0.5));
        __instance.maxChargePowerValue.text = __instance.powerServedSB.ToString();
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMaxTripDroneSliderValueChange))]
    public static bool OnMaxTripDroneSliderValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxTripDrones;
        SliderBarPacket.SettingValue = value;
        if (Multiplayer.Session.LocalPlayer.IsClient)
        {
            __instance.maxTripDroneValue.text = value.ToString("0 °");
        }
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMaxTripVesselSliderValueChange))]
    public static bool OnMaxTripVesselSliderValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxTripVessel;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var num = value switch
        {
            > 40.5f => 10000.0f,
            > 20.5f => value * 2f - 20f,
            _ => value
        };
        __instance.maxTripVesselValue.text = num < 9999.0f ? num.ToString("0 ly") : "∞";
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMinDeliverDroneValueChange))]
    public static bool OnMinDeliverDroneValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverDrone;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var num = (int)(value * 10f + 0.5f);
        if (num < 1)
        {
            num = 1;
        }
        __instance.minDeliverDroneValue.text = num.ToString("0") + " %";
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMinDeliverVesselValueChange))]
    public static bool OnMinDeliverVesselValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverVessel;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var num = (int)(value * 10f + 0.5f);
        num = num < 1 ? 1 : num;
        __instance.minDeliverVesselValue.text = num.ToString("0") + " %";
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMaxMiningSpeedChange))]
    public static bool OnMaxMiningSpeedChanged_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxMiningSpeed;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var num = 10000 + (int)(value + 0.5f) * 1000;
        __instance.maxMiningSpeedValue.text = (num / 100).ToString("0") + " %";
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnWarperDistanceValueChange))]
    public static bool OnWarperDistanceValueChange_Prefix(UIControlPanelStationInspector __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        // make minimum value set at 0.5 ly
        if (value < 2f)
        {
            value = 2f;
            __instance.event_lock = true;
            __instance.warperDistanceSlider.value = 2f;
            __instance.event_lock = false;
        }
        SliderBarPacket.SettingIndex = StationUI.EUISettings.WarpDistance;
        SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        float num;
        if (value < 1.5)
        {
            num = 0.2f;
        }
        else if (value < 7.5)
        {
            num = value * 0.5f - 0.5f;
        }
        else if (value < 16.5)
        {
            num = value - 4f;
        }
        else if (value < 20.5)
        {
            num = value * 2f - 20f;
        }
        else
        {
            num = 60.0f;
        }
        __instance.warperDistanceValue.text = num < 10.0 ? num.ToString("0.0 AU") : num.ToString("0 AU");
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnMinPilerValueChange))]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnTechPilerClick))]
    public static void OnPilerCountChange(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.PilerCount, stationComponent.pilerCount);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnWarperNecessaryClick))]
    public static bool OnWarperNecessaryClick_Prefix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        var packet = new StationUI(__instance.factory.planet.id,
            __instance.factory.transport.stationPool[__instance.stationId].id,
            __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.WarperNeeded, 0f);
        Multiplayer.Session.Network.SendPacket(packet);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnIncludeOrbitCollectorClick))]
    public static bool OnIncludeOrbitCollectorClick_Prefix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        var packet = new StationUI(__instance.factory.planet.id,
            __instance.factory.transport.stationPool[__instance.stationId].id,
            __instance.factory.transport.stationPool[__instance.stationId].gid, StationUI.EUISettings.IncludeCollectors, 0f);
        Multiplayer.Session.Network.SendPacket(packet);
        return Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnDroneIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnDroneIconClick_Prefix(UIControlPanelStationInspector __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.idleDroneCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnDroneIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnDroneIconClick_Postfix(UIControlPanelStationInspector __instance, int __state)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        if (__state == stationComponent.idleDroneCount)
        {
            return;
        }
        var droneCount = stationComponent.idleDroneCount + stationComponent.workDroneCount;
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.SetDroneCount, droneCount);
        Multiplayer.Session.Network.SendPacket(packet);
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return;
        }
        // Revert drone count until host verify
        stationComponent.idleDroneCount = __state;
        __instance.droneIconButton.button.interactable = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnDroneAutoReplenishButtonClick))]
    public static void OnDroneAutoReplenishButtonClick_Postfix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.DroneAutoReplenish, stationComponent.droneAutoReplenish ? 1f : 0f);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnShipIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnShipIconClick_Prefix(UIControlPanelStationInspector __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.idleShipCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnShipIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnShipIconClick_Postfix(UIControlPanelStationInspector __instance, int __state)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        if (__state == stationComponent.idleShipCount)
        {
            return;
        }
        var ShipCount = stationComponent.idleShipCount + stationComponent.workShipCount;
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.SetShipCount, ShipCount);
        Multiplayer.Session.Network.SendPacket(packet);
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return;
        }
        // Revert ship count until host verify
        stationComponent.idleShipCount = __state;
        __instance.shipIconButton.button.interactable = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnShipAutoReplenishButtonClick))]
    public static void OnShipAutoReplenishButtonClick_Postfix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.ShipAutoReplenish, stationComponent.shipAutoReplenish ? 1f : 0f);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnWarperIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnWarperIconClick_Prefix(UIControlPanelStationInspector __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.warperCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnWarperIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnWarperIconClick_Postfix(UIControlPanelStationInspector __instance, int __state)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        if (__state == stationComponent.warperCount)
        {
            return;
        }
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.SetWarperCount, stationComponent.warperCount);
        Multiplayer.Session.Network.SendPacket(packet);
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return;
        }
        // Revert warper count until host verify
        stationComponent.warperCount = __state;
        __instance.warperIconButton.button.interactable = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnGroupButtonClick))]
    public static void OnGroupButtonClick_Postfix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        if (__instance.stationId == 0 || __instance.factory == null)
        {
            return;
        }
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.RemoteGroupMask, BitConverter.Int64BitsToDouble(stationComponent.remoteGroupMask));
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnBehaviorBomboBoxItemIndexChange))]
    public static void OnBehaviorBomboBoxItemIndexChange_Postfix(UIControlPanelStationInspector __instance)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }

        if (__instance.stationId == 0 || __instance.factory == null)
        {
            return;
        }
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        var packet = new StationUI(__instance.factory.planet.id, stationComponent.id, stationComponent.gid,
            StationUI.EUISettings.RoutePriority, (int)stationComponent.routePriority);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector._OnOpen))]
    public static void _OnOpen_Postfix(UIControlPanelStationInspector __instance)
    {
        if (!Multiplayer.IsActive || __instance.transport == null)
        {
            return;
        }

        lastUpdateGametick = 0;
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        SliderBarPacket = new StationUI(__instance.factory.planet.id, stationComponent.id,
            stationComponent.gid, StationUI.EUISettings.None, 0);
        Multiplayer.Session.StationsUI.StorageMaxChangeId = -1;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector._OnUpdate))]
    public static void _OnUpdate_Prefix(UIControlPanelStationInspector __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        // When releasing left mouse button
        if (Input.GetMouseButtonUp(0))
        {
            if (SliderBarPacket.SettingIndex != StationUI.EUISettings.None)
            {
                // Send SliderBarPacket when left mouse button is released
                Multiplayer.Session.Network.SendPacket(SliderBarPacket);
                SliderBarPacket.SettingIndex = StationUI.EUISettings.None;
            }
            if (Multiplayer.Session.StationsUI.StorageMaxChangeId >= 0)
            {
                // Do the job in UIStationStorage.OnMaxSliderValueChange()
                var index = Multiplayer.Session.StationsUI.StorageMaxChangeId;
                var val = __instance.storageUIs[index].maxSlider.value;
                var stationStore = __instance.transport.stationPool[__instance.stationId].storage[index];
                __instance.transport.SetStationStorage(__instance.stationId, index, stationStore.itemId,
                    (int)(val * 100f + 0.5f), stationStore.localLogic, stationStore.remoteLogic, GameMain.mainPlayer);

                // In client side, preserve displaying slider value until host response
                Multiplayer.Session.StationsUI.StorageMaxChangeId = Multiplayer.Session.LocalPlayer.IsHost ? -1 : -2;
            }
        }

        // Request for remoteOrder update every 60tick
        if (!Multiplayer.Session.LocalPlayer.IsClient || GameMain.gameTick - lastUpdateGametick <= 60)
        {
            return;
        }
        var gid = __instance.transport?.stationPool?[__instance.stationId].gid ?? 0;
        if (gid > 0)
        {
            Multiplayer.Session.Network.SendPacket(new RemoteOrderUpdate(gid, Array.Empty<int>()));
        }
        lastUpdateGametick = GameMain.gameTick;

        return;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelStationInspector.OnStationIdChange))]
    public static void OnStationIdChange_Postfix(UIControlPanelStationInspector __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        __instance.warperIconButton.button.interactable = true;
        __instance.shipIconButton.button.interactable = true;
        __instance.droneIconButton.button.interactable = true;
    }
}
