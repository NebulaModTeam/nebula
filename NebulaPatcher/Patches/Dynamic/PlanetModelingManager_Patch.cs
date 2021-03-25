using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using System.Collections.Generic;
using System.Linq;
using NebulaModel.Packets.Universe;
using UnityEngine;

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
            if (GameMain.data.dysonSpheres[star.index] == null)
            {
                Log.Info($"Requesting DysonSphere for system {star.displayName} (Index: {star.index})");
                LocalPlayer.SendPacket(new DysonSphereLoadRequest(star.index));
            }
            return false;
        }

        private static void InternalLoadPlanetsRequestGenerator(PlanetData[] planetsToLoad)
        {
            lock (PlanetModelingManager.genPlanetReqList)
            {
                List<int> planetsToRequest = new List<int>();

                foreach (PlanetData planet in planetsToLoad)
                {
                    planet.wanted = true;
                    if (planet.loaded || planet.loading)
                        continue;

                    planet.loading = true;

                    Log.Info($"Requesting planet model for {planet.name} (ID: {planet.id}) from host");
                    planetsToRequest.Add(planet.id);
                }

                if (planetsToRequest.Any())
                {
                    LocalPlayer.SendPacket(new PlanetDataRequest(planetsToRequest.ToArray()));
                }
            }
        }
    }
    [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
    public class PlanetModelingManager_Patch2
    {
        public static bool Prefix(PlanetData planet)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            //if we are the client we always need to call GetOrCreateFactory() as this is where we handle the FactoryData received from the server
            // NOTE: currentFactingStage is a private field so i need to use the refstub for now
            int currentFactingStage = (int)AccessTools.Field(typeof(PlanetModelingManager), "currentFactingStage").GetValue(null);
            if (planet.factory != null && currentFactingStage == 0)
            {
                GameMain.data.GetOrCreateFactory(planet);
            }
            return true;
        }

        // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information
        [HarmonyPatch(typeof(GameData), "OnActivePlanetLoaded")]
        class GameData_Patch2
        {
            public static bool Prefix(GameData __instance, PlanetData planet)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                if (planet != null)
                {
                    if (planet.factoryLoaded)
                    {
                        __instance.OnActivePlanetFactoryLoaded(planet);
                    }
                    else
                    {
                        planet.LoadFactory();
                        planet.onFactoryLoaded += __instance.OnActivePlanetFactoryLoaded;
                    }
                }
                planet.onLoaded -= __instance.OnActivePlanetLoaded;
                return false;
            }
        }
        // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information
        [HarmonyPatch(typeof(GameData), "OnActivePlanetFactoryLoaded")]
        class GameData_Patch3
        {
            public static bool Prefix(GameData __instance, PlanetData planet)
            {
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                if (planet != null)
                {
                    if (GameMain.gameTick == 0L && DSPGame.SkipPrologue)
                    {
                        GameData_Patch3_Helper.InitLandingPlace(__instance, planet);
                    }
                    // now set localPlanet and planetId
                    AccessTools.Property(typeof(GameData), "localPlanet").SetValue(GameMain.data, planet, null);
                    __instance.mainPlayer.planetId = planet.id;
                }
                planet.onFactoryLoaded -= __instance.OnActivePlanetFactoryLoaded;
                return false;
            }
        }
        class GameData_Patch3_Helper
        {
            public static void InitLandingPlace(GameData gameData, PlanetData planet)
            {
                Vector3 birthPoint = planet.birthPoint;
                Quaternion quaternion = Maths.SphericalRotation(birthPoint, 0f);
                gameData.mainPlayer.transform.localPosition = birthPoint;
                gameData.mainPlayer.transform.localRotation = quaternion;
                gameData.mainPlayer.transform.localScale = Vector3.one;
                gameData.mainPlayer.uPosition = (Vector3)planet.uPosition + planet.runtimeRotation * birthPoint;
                gameData.mainPlayer.uRotation = planet.runtimeRotation * quaternion;
                gameData.mainPlayer.uVelocity = VectorLF3.zero;
                gameData.mainPlayer.controller.velocityOnLanding = Vector3.zero;
            }
        }
    }
}
