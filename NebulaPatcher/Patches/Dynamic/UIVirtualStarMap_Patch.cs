using HarmonyLib;
using NebulaModel.Logger;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIVirtualStarmap))]
    class UIVirtualStarMap_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIVirtualStarmap._OnLateUpdate))]
        public static bool _OnLateUpdate_Prefix(UIVirtualStarmap __instance)
        {
            // we set this in the transpiler once entered the solarsystem details, so prevent the normal call here
            if(__instance.clickText != "")
            {
                //return false;
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
