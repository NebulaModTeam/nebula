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
}

