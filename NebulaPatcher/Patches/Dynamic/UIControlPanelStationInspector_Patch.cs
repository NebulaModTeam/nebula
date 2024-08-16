#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelStationInspector))]
internal class UIControlPanelStationInspector_Patch
{
    [HarmonyPostfix, HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(UIControlPanelStationInspector._OnOpen))]
    public static void OnOpen_Postfix(UIControlPanelStationInspector __instance)
    {
        if (!Multiplayer.IsActive) return;

        // Temporarily disable the station window, as we need to deal with remote station and sync in the future
        __instance._Close();
    }
}
