#region

using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelAdvancedMinerEntry))]
internal class UIControlPanelAdvancedMinerEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelAdvancedMinerEntry.OnSetTarget))]
    public static bool OnSetTarget_Prefix(UIControlPanelAdvancedMinerEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        var factory = GameMain.data.galaxy.PlanetById(__instance.target.astroId).factory;
        if (factory == null)
        {
            LCPObjectEntryRequest.Instance.Set(__instance, true);
            Multiplayer.Session.Client.SendPacket(LCPObjectEntryRequest.Instance);
            __instance.station = Multiplayer.Session.StationsUI.DummyStationStoreContainer;
            __instance.storageItem.station = __instance.station;
            __instance.storageItem.SetVisible(false);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelAdvancedMinerEntry._OnUpdate))]
    public static bool Update_Prefix(UIControlPanelAdvancedMinerEntry __instance)
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
    [HarmonyPatch(nameof(UIControlPanelAdvancedMinerEntry.isLocal), MethodType.Getter)]
    public static bool IsLocal_Prefix(UIControlPanelAdvancedMinerEntry __instance, ref bool __result)
    {
        if (__instance.factory == null)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
