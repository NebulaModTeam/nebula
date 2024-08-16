#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelStationEntry))]
internal class UIControlPanelStationEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationEntry.OnFillNecessaryButtonClick))]
    public static bool OnFillNecessaryButtonClick_Prefix()
    {
        if (!Multiplayer.IsActive) return true;

        // Temporarily disable fill item button. We will sync in the future
        return false;
    }
}
