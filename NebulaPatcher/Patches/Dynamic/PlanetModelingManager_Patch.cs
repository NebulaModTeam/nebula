using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using System.Collections.Generic;
using System.Linq;
using NebulaModel.Packets.Universe;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetModelingManager))]
    public class PlanetModelingManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RequestLoadPlanetFactory")]
        public static bool RequestLoadPlanetFactory(PlanetData planet)
        {
            // Run the original method if this is the master client or in single player games
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            // Check to make sure it's not already loaded
            if (planet.factoryLoaded || planet.factoryLoading)
                return false;

            // They appear to have conveniently left this flag in for us, but they don't use it anywhere
            planet.factoryLoading = true;

            // Request factory
            Log.Info($"Requested factory for planet {planet.name} (ID: {planet.id}) from host");
            LocalPlayer.SendPacket(new FactoryLoadRequest(planet.id));

            // Skip running the actual method
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RequestLoadPlanet")]
        public static bool RequestLoadPlanet(PlanetData planet)
        {
            // NOTE: This does not appear to ever be called in the game code, but just in case, let's override it
            // RequestLoadStar takes care of these instead currently

            // Run the original method if this is the master client or in single player games
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            InternalLoadPlanetsRequestGenerator(new[] { planet });

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RequestLoadStar")]
        public static bool RequestLoadStar(StarData star)
        {
            // Run the original method if this is the master client or in single player games
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            InternalLoadPlanetsRequestGenerator(star.planets);

            // Request initial dysonSphere data
            Log.Info($"Requesting DysonSphere for system {star.displayName} (Index: {star.index})");
            LocalPlayer.SendPacket(new DysonSphereLoadRequest(star.index));

            return false;
        }

        private static void InternalLoadPlanetsRequestGenerator(PlanetData[] planetsToLoad)
        {
            lock (PlanetModelingManager.genPlanetReqList)
            {
                List<int> planetsToRequest = new List<int>();

                foreach(PlanetData planet in planetsToLoad)
                {
                    planet.wanted = true;
                    if (planet.loaded || planet.loading)
                        continue;

                    planet.loading = true;

                    Log.Info($"Requesting planet model for {planet.name} (ID: {planet.id}) from host");
                    planetsToRequest.Add(planet.id);
                }

                if(planetsToRequest.Any())
                {
                    LocalPlayer.SendPacket(new PlanetDataRequest(planetsToRequest.ToArray()));
                }
            }
        }
    }
}
