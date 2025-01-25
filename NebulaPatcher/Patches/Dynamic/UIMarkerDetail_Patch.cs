#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIMarkerDetail))]
internal class UIMarkerDetail_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIMarkerDetail), nameof(UIMarkerDetail.SetInspectPlanet))]
    public static void SetInspectPlanet_Prefix(UIMarkerDetail __instance)
    {
        // When teleporting to another planet, the inspect planet factory can be null
        // So set the inspectPlanet to null first in here
        if (__instance.inspectPlanet != null && __instance.inspectPlanet.factory == null)
        {
            __instance.inspectPlanet = null;
        }
    }
}
