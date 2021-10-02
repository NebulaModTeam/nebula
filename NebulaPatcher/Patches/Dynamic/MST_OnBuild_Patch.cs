using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MST_OnBuild))]
    public class MST_OnBuild_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MST_OnBuild.OnGameTick))]
        public static bool OnGameTick_Prefix(MST_OnBuild __instance)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            if (__instance.localPlanetFactory != null && __instance.localPlanetFactory.planet == null)
            {
                __instance.localPlanetFactory.onBuild -= __instance.OnBuild;
                __instance.localPlanetFactory = null;
                return false;
            }

            return true;
        }
    }
}