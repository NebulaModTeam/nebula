using HarmonyLib;
using NebulaWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarmap))]
    class UIStarmap_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("_OnLateUpdate")]
        public static void OnLateUpdate_Postfix(UIStarmap __instance)
        {
            SimulatedWorld.RenderPlayerNameTagsOnStarmap(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void OnClose_Postfix(UIStarmap __instance)
        {
            SimulatedWorld.ClearPlayerNameTagsOnStarmap();
        }
    }
}
