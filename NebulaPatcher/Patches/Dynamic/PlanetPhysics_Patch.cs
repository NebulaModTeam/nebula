using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetPhysics))]
    class PlanetPhysics_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetPhysics.RemoveLinkedColliderData))]
        public static bool RemoveLinkedColliderData_Prefix(PlanetPhysics __instance)
        {
            //Collider does not need to be removed if player is not on the planet
            if (SimulatedWorld.Instance.Initialized && __instance.planet.id != GameMain.mainPlayer.planetId)
            {
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetPhysics.RemoveColliderData))]
        public static bool RemoveColliderData_Prefix(PlanetPhysics __instance)
        {
            //Collider does not need to be removed if player is not on the planet
            if (SimulatedWorld.Instance.Initialized && __instance.planet.id != GameMain.mainPlayer.planetId)
            {
                return false;
            }
            return true;
        }
    }
}
