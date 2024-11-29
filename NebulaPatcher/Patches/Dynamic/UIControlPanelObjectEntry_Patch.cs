#region

using System.Collections.Generic;
using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch(typeof(UIControlPanelObjectEntry))]
internal class UIControlPanelObjectEntry_Patch
{
    [HarmonyReversePatch(HarmonyReversePatchType.Original)]
    [HarmonyPatch(nameof(UIControlPanelObjectEntry._OnUpdate))]
    public static void OnUpdate(UIControlPanelObjectEntry entry)
    {
        // Use HarmonyReversePatch to call base._OnUpdate as base is not available in static function
        _ = Transpiler(null);
        return;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelObjectEntry.OnSelectButtonClick))]
    public static void OnSelectButtonClick_Postfix(UIControlPanelObjectEntry __instance)
    {
        if (!__instance.isTargetDataValid)
        {
            // The main reason why target data is invalid is due to remote planet is not loaded for client
            // So make a popup here to info the user about this behavior
            UIRealtimeTip.Popup("Can't view remote planet for MP client!".Translate());
        }
    }
}
