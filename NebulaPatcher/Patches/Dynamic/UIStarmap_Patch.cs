using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarmap))]
    class UIStarmap_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("_OnLateUpdate")]
        public static void OnLateUpdate_Postfix(UIStarmap __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                SimulatedWorld.RenderPlayerNameTagsOnStarmap(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void OnClose_Postfix(UIStarmap __instance)
        {
            if (SimulatedWorld.Initialized)
            {
                SimulatedWorld.ClearPlayerNameTagsOnStarmap();
            }
        }
    }
}
