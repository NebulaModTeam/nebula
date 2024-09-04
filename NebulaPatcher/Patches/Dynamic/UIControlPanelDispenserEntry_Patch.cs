#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelDispenserEntry))]
internal class UIControlPanelDispenserEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelDispenserEntry.OnSetTarget))]
    public static bool OnSetTarget_Prefix(UIControlPanelDispenserEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        var planet = GameMain.data.galaxy.PlanetById(__instance.target.astroId);
        var factory = planet.factory;
        if (factory == null)
        {
            LCPObjectEntryRequest.Instance.Set(__instance, true);
            Multiplayer.Session.Client.SendPacket(LCPObjectEntryRequest.Instance);
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelDispenserEntry._OnUpdate))]
    public static bool Update_Prefix(UIControlPanelDispenserEntry __instance)
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
    [HarmonyPatch(nameof(UIControlPanelDispenserEntry.isLocal), MethodType.Getter)]
    public static bool IsLocal_Prefix(UIControlPanelDispenserEntry __instance, ref bool __result)
    {
        if (__instance.factory == null)
        {
            __result = false;
            return false;
        }
        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelDispenserEntry.OnFillNecessaryButtonClick))]
    public static bool OnFillNecessaryButtonClick_Prefix(UIControlPanelDispenserEntry __instance)
    {
        if (!Multiplayer.IsActive) return true;

        // Temporarily disable fill item button. We will sync in the future
        return false;
    }
}
