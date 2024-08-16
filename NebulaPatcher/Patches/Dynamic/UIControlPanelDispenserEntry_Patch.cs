#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelDispenserEntry))]
internal class UIControlPanelDispenserEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelDispenserEntry.OnFillNecessaryButtonClick))]
    public static bool OnFillNecessaryButtonClick_Prefix(UIControlPanelDispenserEntry __instance)
    {
        if (!Multiplayer.IsActive) return true;

        // Temporarily disable fill item button. We will sync in the future
        return false;
    }
}
