using HarmonyLib;
using NebulaPatcher.Patches.Transpilers;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIVirtualStarmap))]
    class UIVirtualStarMap_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIVirtualStarmap._OnLateUpdate))]
        public static bool _OnLateUpdate_Prefix(UIVirtualStarmap __instance)
        {
            // reset the spam protector if no press is recognized to enable solar system details again.
            if (!VFInput.rtsConfirm.pressing)
            {
                UIVirtualStarmap_Transpiler.pressSpamProtector = false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIVirtualStarmap.OnGalaxyDataReset))]
        public static bool OnGalaxyDataReset_Prefix(UIVirtualStarmap __instance)
        {
            __instance.clickText = ""; // reset to vanilla

            foreach (UIVirtualStarmap.ConnNode connNode in __instance.connPool)
            {
                connNode.lineRenderer.positionCount = 2;
            }

            return true;
        }
    }
}
