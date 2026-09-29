#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIMarkerDetail))]
internal class UIMarkerDetail_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIMarkerDetail), nameof(UIMarkerDetail.SetInspectPlanet))]
    public static bool SetInspectPlanet_Prefix(UIMarkerDetail __instance, PlanetData _planet)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        // When teleporting to another planet or loading, the target planet factory can be null
        // Prevent vanilla crash and clear current inspection safely
        if (_planet != null && _planet.factory == null)
        {
            __instance.inspectPlanet = null;
            __instance.allNode?.Clear();
            return false;
        }

        if (__instance.inspectPlanet != null && __instance.inspectPlanet.factory == null)
        {
            __instance.inspectPlanet = null;
        }

        return true;
    }
}
