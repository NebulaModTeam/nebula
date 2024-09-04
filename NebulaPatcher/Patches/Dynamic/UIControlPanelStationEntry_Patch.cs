#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

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
    public static bool OnFillNecessaryButtonClick_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Temporarily disable fill item button. We will sync in the future
        return false;
    }
}
