﻿using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using System.Collections.Generic;
using System.Linq;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetModelingManager))]
    public class PlanetModelingManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetModelingManager.RequestLoadPlanetFactory))]
        public static bool RequestLoadPlanetFactory_Prefix(PlanetData planet)
        {
            // Run the original method if this is the master client or in single player games
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            // Check to make sure it's not already loaded
            if (planet.factoryLoaded || planet.factoryLoading)
            {
                return false;
            }

            // if client is still in lobby we dont need the factory data at all, so dont request it.
            if (Multiplayer.Session.IsInLobby)
            {
                return false;
            }

            // They appear to have conveniently left this flag in for us, but they don't use it anywhere
            planet.factoryLoading = true;

            // do this here to match the patch in GPUInstancingManager_Patch.cs
            // as we sync entity placement in realtime when players change something
            // we only need to request the full factory if we never received it before
            if (planet.factory != null)
            {
                PlanetModelingManager.currentFactingPlanet = planet;
                PlanetModelingManager.currentFactingStage = 0;
                return false;
            }

            // Request factory
            Log.Info($"Requested factory for planet {planet.name} (ID: {planet.id}) from host");
            Multiplayer.Session.Network.SendPacket(new FactoryLoadRequest(planet.id));

            NebulaModAPI.OnPlanetLoadRequest?.Invoke(planet.id);

            // Skip running the actual method
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetModelingManager.RequestLoadPlanet))]
        public static bool RequestLoadPlanet_Prefix(PlanetData planet)
        {
            // NOTE: This does not appear to ever be called in the game code, but just in case, let's override it
            // RequestLoadStar takes care of these instead currently

            // Run the original method if this is the master client or in single player games
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            InternalLoadPlanetsRequestGenerator(new[] { planet });

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlanetModelingManager.RequestLoadStar))]
        public static bool RequestLoadStar_Prefix(StarData star)
        {
            // Run the original method if this is the master client or in single player games
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }

            InternalLoadPlanetsRequestGenerator(star.planets);

            Multiplayer.Session.DysonSpheres.UnloadRemoteDysonSpheres();
            // Request initial dysonSphere data
            if (GameMain.data.dysonSpheres[star.index] == null)
            {
                Multiplayer.Session.DysonSpheres.RequestingIndex = star.index;
                Log.Info($"Requesting DysonSphere for system {star.displayName} (Index: {star.index})");
                Multiplayer.Session.Network.SendPacket(new DysonSphereLoadRequest(star.index, DysonSphereRequestEvent.Load));
            }

            NebulaModAPI.OnStarLoadRequest?.Invoke(star.index);

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
                    {
                        continue;
                    }

                    planet.loading = true;

                    Log.Info($"Requesting planet model for {planet.name} (ID: {planet.id}) from host");
                    planetsToRequest.Add(planet.id);
                }

                if (planetsToRequest.Any())
                {
                    Multiplayer.Session.Network.SendPacket(new PlanetDataRequest(planetsToRequest.ToArray()));
                }
            }
        }
    }
}
