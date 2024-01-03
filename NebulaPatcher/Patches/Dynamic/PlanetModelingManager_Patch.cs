#region

using System;
using System.Collections.Generic;
using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

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
        try
        {
            NebulaModAPI.OnPlanetLoadRequest?.Invoke(planet.id);
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnPlanetLoadRequest error:\n" + e);
        }

        // Request factory
        Log.Info($"Requested factory for planet {planet.name} (ID: {planet.id}) from host");
        Multiplayer.Session.Network.SendPacket(new FactoryLoadRequest(planet.id));

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
            Multiplayer.Session.DysonSpheres.RequestDysonSphere(star.index, false);
        }

        try
        {
            NebulaModAPI.OnStarLoadRequest?.Invoke(star.index);
        }
        catch (Exception e)
        {
            Log.Error("NebulaModAPI.OnStarLoadRequest error:\n" + e);
        }

        return false;
    }

    private static void InternalLoadPlanetsRequestGenerator(IEnumerable<PlanetData> planetsToLoad)
    {
        lock (PlanetModelingManager.genPlanetReqList)
        {
            var planetsToRequest = new List<int>();

            foreach (var planet in planetsToLoad)
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

            if (planetsToRequest.Count == 0)
            {
                return;
            }
            // Make local planet load first
            var localPlanetId = Multiplayer.Session.LocalPlayer?.Data?.LocalPlanetId ?? -1;
            if (localPlanetId == -1)
            {
                localPlanetId = UIVirtualStarmap_Transpiler.CustomBirthPlanet;
            }

            if (planetsToRequest.Remove(localPlanetId))
            {
                planetsToRequest.Insert(0, localPlanetId);
            }
            Multiplayer.Session.Network.SendPacket(new PlanetDataRequest(planetsToRequest.ToArray()));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetModelingManager.RequestCalcPlanet))]
    public static bool RequestCalcPlanet_Prefix(PlanetData planet)
    {
        // Run the original method if this is the master client or in single player games
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        RequestCalcPlanet(planet);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlanetModelingManager.RequestCalcStar))]
    public static bool RequestCalcStar_Prefix(StarData star)
    {
        // Run the original method if this is the master client or in single player games
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        foreach (var planet in star.planets)
        {
            RequestCalcPlanet(planet);
        }
        return false;
    }

    private static void RequestCalcPlanet(PlanetData planet)
    {
        if (planet.calculated || planet.calculating || planet.data != null)
        {
            return;
        }
        if (planet.loaded || planet.loading)
        {
            return;
        }
        planet.calculating = true;
        Multiplayer.Session.Network.SendPacket(new PlanetDetailRequest(planet.id));
    }
}
