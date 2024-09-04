#region

using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelStationEntry))]
internal class UIControlPanelStationEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationEntry.OnSetTarget))]
    public static bool OnSetTarget_Prefix(UIControlPanelStationEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        var factory = GameMain.data.galaxy.PlanetById(__instance.target.astroId).factory;
        if (factory == null)
        {
            LCPObjectEntryRequest.Instance.Set(__instance, true);
            Multiplayer.Session.Client.SendPacket(LCPObjectEntryRequest.Instance);
            __instance.station = Multiplayer.Session.StationsUI.DummyStationStoreContainer;
            __instance.storageItem0.station = __instance.station;
            __instance.storageItem1.station = __instance.station;
            __instance.storageItem2.station = __instance.station;
            __instance.storageItem3.station = __instance.station;
            __instance.storageItem4.station = __instance.station;
            __instance.storageItem0.SetVisible(false);
            __instance.storageItem1.SetVisible(false);
            __instance.storageItem2.SetVisible(false);
            __instance.storageItem3.SetVisible(false);
            __instance.storageItem4.SetVisible(false);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationEntry._OnUpdate))]
    public static bool Update_Prefix(UIControlPanelStationEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        if (__instance.factory != null) return true;

        UIControlPanelObjectEntry_Patch.OnUpdate(__instance);
        __instance.viewToTargetButton.button.interactable = __instance.isLocal;
        if (UIControlPanelWindow_Patch.UpdateTimer % 60 == 0)
        {
            // Request content update every 1s
            LCPObjectEntryRequest.Instance.Set(__instance, false);
            Multiplayer.Session.Client.SendPacket(LCPObjectEntryRequest.Instance);
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationEntry.isLocal), MethodType.Getter)]
    public static bool IsLocal_Prefix(UIControlPanelAdvancedMinerEntry __instance, ref bool __result)
    {
        if (__instance.factory == null)
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationEntry.OnFillNecessaryButtonClick))]
    public static bool OnFillNecessaryButtonClick_Prefix(UIControlPanelStationEntry __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (__instance.factory == null || __instance.station == null)
        {
            UIRealtimeTip.Popup("Unavailable".Translate());
            return false;
        }

        var packet = new StationUI()
        {
            PlanetId = __instance.factory.planetId,
            StationId = __instance.station.id,
            StationGId = __instance.station.gid
        };
        var text = "";
        if (__instance.station.isStellar)
        {
            int shortAge, previousCount;

            previousCount = __instance.station.idleDroneCount;
            shortAge = __instance.station.workDroneDatas.Length - (__instance.station.idleDroneCount + __instance.station.workDroneCount);
            UIControlPanelObjectEntry.ReplenishItems(5001, shortAge, ref __instance.station.idleDroneCount, ref text);
            packet.SettingIndex = StationUI.EUISettings.SetDroneCount;
            packet.SettingValue = (__instance.station.idleDroneCount + __instance.station.workDroneCount);
            Multiplayer.Session.Network.SendPacket(packet);
            if (Multiplayer.Session.IsClient) __instance.station.idleDroneCount = previousCount; // Wait for server to authorize

            previousCount = __instance.station.idleShipCount;
            shortAge = __instance.station.workShipDatas.Length - (__instance.station.idleShipCount + __instance.station.workShipCount);
            UIControlPanelObjectEntry.ReplenishItems(5002, shortAge, ref __instance.station.idleShipCount, ref text);
            packet.SettingIndex = StationUI.EUISettings.SetShipCount;
            packet.SettingValue = (__instance.station.idleShipCount + __instance.station.workShipCount);
            Multiplayer.Session.Network.SendPacket(packet);
            if (Multiplayer.Session.IsClient) __instance.station.idleShipCount = previousCount; // Wait for server to authorize

            previousCount = __instance.station.warperCount;
            shortAge = __instance.station.warperMaxCount - __instance.station.warperCount;
            UIControlPanelObjectEntry.ReplenishItems(1210, shortAge, ref __instance.station.warperCount, ref text);
            packet.SettingIndex = StationUI.EUISettings.SetWarperCount;
            packet.SettingValue = __instance.station.warperCount;
            Multiplayer.Session.Network.SendPacket(packet);
            if (Multiplayer.Session.IsClient) __instance.station.warperCount = previousCount; // Wait for server to authorize
        }
        else
        {
            var previousCount = __instance.station.idleDroneCount;
            var shortAge = __instance.station.workDroneDatas.Length - (__instance.station.idleDroneCount + __instance.station.workDroneCount);
            UIControlPanelObjectEntry.ReplenishItems(5001, shortAge, ref __instance.station.idleDroneCount, ref text);
            packet.SettingIndex = StationUI.EUISettings.SetDroneCount;
            packet.SettingValue = (__instance.station.idleDroneCount + __instance.station.workDroneCount);
            Multiplayer.Session.Network.SendPacket(packet);
            if (Multiplayer.Session.IsClient) __instance.station.idleDroneCount = previousCount; // Wait for server to authorize
        }
        if (!string.IsNullOrEmpty(text))
        {
            UIRealtimeTip.Popup(text, false, 0);
            VFAudio.Create("equip-1", GameMain.mainPlayer.transform, Vector3.zero, true, 4, -1, -1L);
        }
        return false;
    }
}
