#region

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;
// ReSharper disable RedundantAssignment

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStationWindow))]
internal class UIStationWindow_Patch
{
    private static long lastUpdateGametick;

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
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        StringBuilderUtility.WriteKMG(__instance.powerServedSB, 8, (long)(3000000.0 * value + 0.5));
        __instance.maxChargePowerValue.text = __instance.powerServedSB.ToString();
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
        if (Multiplayer.Session.LocalPlayer.IsClient)
        {
            __instance.maxTripDroneValue.text = value.ToString("0 °");
        }
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
    [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverDroneValueChange))]
    public static bool OnMinDeliverDroneValueChange_Prefix(UIStationWindow __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverDrone;
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
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
    [HarmonyPatch(nameof(UIStationWindow.OnMinDeliverVesselValueChange))]
    public static bool OnMinDeliverVesselValueChange_Prefix(UIStationWindow __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MinDeliverVessel;
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
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
    [HarmonyPatch(nameof(UIStationWindow.OnMaxMiningSpeedChange))]
    public static bool OnMaxMiningSpeedChanged_Prefix(UIStationWindow __instance, float value)
    {
        if (__instance.event_lock || !Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return true;
        }
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.MaxMiningSpeed;
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
        if (!Multiplayer.Session.LocalPlayer.IsClient)
        {
            return Multiplayer.Session.LocalPlayer.IsHost;
        }
        var num = 10000 + (int)(value + 0.5f) * 1000;
        __instance.maxMiningSpeedValue.text = (num / 100).ToString("0") + " %";
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
        // make minimum value set at 0.5 ly
        if (value < 2f)
        {
            value = 2f;
            __instance.event_lock = true;
            __instance.warperDistanceSlider.value = 2f;
            __instance.event_lock = false;
        }
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingIndex = StationUI.EUISettings.WarpDistance;
        Multiplayer.Session.StationsUI.SliderBarPacket.SettingValue = value;
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
    [HarmonyPatch(nameof(UIStationWindow.OnMinPilerValueChange))]
    [HarmonyPatch(nameof(UIStationWindow.OnTechPilerClick))]
    public static void OnPilerCountChange(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnWarperNecessaryClick))]
    public static bool OnWarperNecessaryClick_Prefix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnIncludeOrbitCollectorClick))]
    public static bool OnIncludeOrbitCollectorClick_Prefix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnDroneIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnDroneIconClick_Prefix(UIStationWindow __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.idleDroneCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationWindow.OnDroneIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnDroneIconClick_Postfix(UIStationWindow __instance, int __state)
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
    [HarmonyPatch(nameof(UIStationWindow.OnDroneAutoReplenishButtonClick))]
    public static void OnDroneAutoReplenishButtonClick_Postfix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnShipIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnShipIconClick_Prefix(UIStationWindow __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.idleShipCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationWindow.OnShipIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnShipIconClick_Postfix(UIStationWindow __instance, int __state)
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
    [HarmonyPatch(nameof(UIStationWindow.OnShipAutoReplenishButtonClick))]
    public static void OnShipAutoReplenishButtonClick_Postfix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnWarperIconClick))]
    [HarmonyPriority(Priority.First)]
    public static void OnWarperIconClick_Prefix(UIStationWindow __instance, ref int __state)
    {
        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        __state = stationComponent.warperCount;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationWindow.OnWarperIconClick))]
    [HarmonyPriority(Priority.Last)]
    public static void OnWarperIconClick_Postfix(UIStationWindow __instance, int __state)
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
    [HarmonyPatch(nameof(UIStationWindow.OnGroupButtonClick))]
    public static void OnGroupButtonClick_Postfix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow.OnBehaviorBomboBoxItemIndexChange))]
    public static void OnBehaviorBomboBoxItemIndexChange_Postfix(UIStationWindow __instance)
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
    [HarmonyPatch(nameof(UIStationWindow._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIStationWindow __instance)
    {
        if (!Multiplayer.IsActive || __instance.transport == null)
        {
            return;
        }

        var stationComponent = __instance.transport.stationPool[__instance.stationId];
        Multiplayer.Session.StationsUI.SliderBarPacket = new StationUI(__instance.factory.planet.id, stationComponent.id,
            stationComponent.gid, StationUI.EUISettings.None, 0);
        Multiplayer.Session.StationsUI.StorageMaxChangeId = -1;
        // We want OnNameInputSubmit only call on nameInput.onEndEdit, so remove listener on onValueChanged
        __instance.nameInput.onValueChanged.RemoveAllListeners();
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        lastUpdateGametick = GameMain.gameTick;

        // Stage 0 : Hide UI elements until sync data arrive
        __instance.titleText.text = "Loading...";
        foreach (var t in __instance.storageUIs)
        {
            t._Close();
            t.ClosePopMenu();
        }
        __instance.panelDown.SetActive(false);

        // for some reason advance miner has planetId set to 0, so we use UI's factory planetId
        Multiplayer.Session.Network.SendPacket(new StationUIInitialSyncRequest(__instance.factory.planetId, stationComponent.id,
            stationComponent.gid));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationWindow._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
            return true;
        }
        var gid = __instance.transport?.stationPool?[__instance.stationId].gid ?? 0;
        if (gid > 0)
        {
            Multiplayer.Session.Network.SendPacket(new RemoteOrderUpdate(gid, Array.Empty<int>()));
        }
        lastUpdateGametick = GameMain.gameTick;

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationWindow.OnStationIdChange))]
    public static void OnStationIdChange_Postfix(UIStationWindow __instance)
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
