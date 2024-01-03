#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaPatcher.Patches.Transpilers;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIVirtualStarmap))]
internal class UIVirtualStarMap_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIVirtualStarmap._OnLateUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnLateUpdate_Prefix()
    {
        // reset the spam protector if no press is recognized to enable solar system details again.
        if (!VFInput.rtsConfirm.pressing)
        {
            UIVirtualStarmap_Transpiler.PressSpamProtector = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIVirtualStarmap.OnGalaxyDataReset))]
    public static void OnGalaxyDataReset_Prefix(UIVirtualStarmap __instance)
    {
        __instance.clickText = ""; // reset to vanilla

        foreach (var connNode in __instance.connPool)
        {
            connNode.lineRenderer.positionCount = 2;
        }
    }
}
