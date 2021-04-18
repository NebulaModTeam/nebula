using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData))]
    class ArrivePlanet_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ArrivePlanet")]
        public static bool ArrivePlanet_Prefix(GameData __instance, PlanetData planet)
        {
            // we need to supply our own ArrivePlanet() logic as we load the PlanetFactory from the server (if we are a client at least).
            // due to that we have a time window between the vanilla ArrivePlanet() setting the localPlanet and planetId values and
            // our code loading the factory data.
            // this results in weird planet jumps, so we need to delay this until the factory data is loaded into the game.
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                // if we are the server continue with vanilla logic
                return true;
            }

            // it is very painfull to patch the skip prologue functionality
            // so i apply the patched logic only after that.
            if (!SimulatedWorld.IsGameLoaded)
            {
                return true;
            }
            else // we are past the prologue and initial landing so we can use the patched logic (delaying localPlanet and planetId)
            {

                if (planet == __instance.localPlanet)
                {
                    return false;
                }
                if (__instance.localPlanet != null)
                {
                    __instance.LeavePlanet();
                }
                if (planet != null && !planet.factoryLoading)
                {
                    if (__instance.localStar != planet.star)
                    {
                        __instance.ArriveStar(planet.star);
                    }
                    // skip setting local planet stuff
                    // NOTE: we also need to patch OnActivePlanetLoaded() as it relies on localPlanet but is needed to call LoadFactory() once the planet is loaded
                    if (planet.loaded)
                    {
                        __instance.OnActivePlanetLoaded(planet);
                    }
                    else
                    {
                        planet.onLoaded += __instance.OnActivePlanetLoaded;
                    }
                }
                return false;
            }
        }

        public static bool RefreshMissingMeshes = false;

        [HarmonyPostfix]
        [HarmonyPatch("ArrivePlanet")]
        public static void ArrivePlanet_Postfix(GameData __instance, PlanetData planet)
        {
            RefreshMissingMeshes = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void GameTick_Postfix(GameData __instance)
        {
            if (SimulatedWorld.Initialized && RefreshMissingMeshes && __instance.localPlanet != null)
            {
                PlanetData planetData = __instance.localPlanet;
                
                if (planetData.meshColliders != null) {
                    for (int i = 0; i < planetData.meshColliders.Length; i++)
                    {
                        if (planetData.meshColliders[i] != null && planetData.meshColliders[i].sharedMesh == null)
                        {
                            planetData.meshColliders[i].sharedMesh = planetData.meshes[i];
                        }
                    }
                    RefreshMissingMeshes = false;
                }
            }
        }

    }
}

