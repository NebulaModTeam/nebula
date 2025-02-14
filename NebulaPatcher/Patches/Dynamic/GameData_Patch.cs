#region

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using NebulaAPI;
using NebulaAPI.GameState;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using NebulaWorld.GameStates;
using NebulaWorld.Planet;
using NebulaWorld.Warning;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameData))]
internal class GameData_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.Update))]
    public static void Update_Prefix()
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.IsGameLoaded)
        {
            return;
        }

        Multiplayer.Session.World.RenderPlayerNameTagsInGame();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.GetOrCreateFactory))]
    public static bool GetOrCreateFactory_Prefix(GameData __instance, ref PlanetFactory __result, PlanetData planet)
    {
        // We want the original method to run on the host client or in single player games
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return true;
        }

        // Get the recieved bytes from the remote server that we will import
        if (!Multiplayer.Session.Planets.PendingFactories.TryGetValue(planet.id, out var factoryBytes))
        {
            // If planet.factory is not empty, it may be called by elsewhere beside PlanetModelingManager.LoadingPlanetFactoryMain
            // In this case, just use the original method which will return planet.factory
            if (planet.factory == null)
            {
                // We messed up, just defer to the default behaviour on the client (will cause desync but not outright crash)
                Log.Error("PendingFactories did not have value we wanted, factory will not be synced!");
            }
            return true;
        }

        // Take it off the list, as we will process it now
        Multiplayer.Session.Planets.PendingFactories.Remove(planet.id);

        using (var reader = new BinaryUtils.Reader(factoryBytes))
        {
            int factoryIndex;
            // Import the factory from the given bytes, which will have been gotten or created on the host by the original function
            if (planet.factory == null)
            {
                factoryIndex = GameMain.data.factoryCount++;
                planet.factoryIndex = factoryIndex;
                __instance.factories[factoryIndex] = new PlanetFactory();
                try
                {
                    __instance.factories[factoryIndex].Import(factoryIndex, __instance, reader.BinaryReader.BaseStream, reader.BinaryReader);
                }
                catch (InvalidOperationException e)
                {
                    HandleRemoteDataImportError(e);
                    return false;
                }
                planet.factory = __instance.factories[factoryIndex];
            }
            else
            {
                factoryIndex = planet.factoryIndex;
                try
                {
                    __instance.factories[factoryIndex].Import(factoryIndex, __instance, reader.BinaryReader.BaseStream, reader.BinaryReader);
                }
                catch (InvalidOperationException e)
                {
                    HandleRemoteDataImportError(e);
                    return false;
                }
                planet.factory = __instance.factories[factoryIndex];
            }
            // Initial FactoryProductionStat for other in-game stats checking functions
            if (GameMain.statistics.production.factoryStatPool[factoryIndex] == null)
            {
                GameMain.statistics.production.factoryStatPool[factoryIndex] = new FactoryProductionStat();
                GameMain.statistics.production.factoryStatPool[factoryIndex].Init();
                //Skip the part of setting firstCreateIds
            }
        }

        // Assign the factory to the result
        __result = __instance.factories[planet.factoryIndex];

        // Do not run the original method
        return false;
    }

    // a user reported to receive an error regarding decompressing the lz4 data which breaks the game as the planet factory cant be loaded.
    // it is not known what exactly caused the error but that client seemed to have an instable internet connection.
    // so we give advice in this rare situation to issue a reconnect.
    private static void HandleRemoteDataImportError(InvalidOperationException e)
    {
        WarningManager.DisplayCriticalWarning(
            "Failed to properly decompress and import factory data.\nPlease do a reconnect.\nSee the logfile for more information.");
        Log.Error(
            $"There was an error while decompressing and importing factory data, probably due to an instable internet connection. See full error below.\n{e.StackTrace}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.OnActivePlanetLoaded))]
    public static bool OnActivePlanetLoaded_Prefix(GameData __instance, PlanetData planet)
    {
        // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information

        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
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
                // triggers data request to server
                planet.LoadFactory();
                planet.onFactoryLoaded += __instance.OnActivePlanetFactoryLoaded;
                // lets player walk on empty planet without awkward graphic glitches
                if (planet.physics == null)
                {
                    planet.physics = new PlanetPhysics(planet);
                    planet.physics.Init();
                }
            }
            RefreshMissingMeshes();
        }
        if (planet != null)
        {
            planet.onLoaded -= __instance.OnActivePlanetLoaded;
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.OnActivePlanetFactoryLoaded))]
    public static bool OnActivePlanetFactoryLoaded_Prefix(GameData __instance, PlanetData planet)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }
        RefreshMissingMeshes();
        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            // Resume packet processing when local planet is loaded
            Multiplayer.Session.Network.PacketProcessor.EnablePacketProcessing = true;
            Log.Info("Resume PacketProcessor (OnActivePlanetFactoryLoaded)");
            return true;
        }
        if (planet != null)
        {
            if (GameMain.gameTick == 0L && DSPGame.SkipPrologue)
            {
                InitLandingPlace(__instance, planet);
            }

            // now that we have the factory loaded update it in the planets physics
            planet.physics.raycastLogic.factory = planet.factory;

            planet.onFactoryLoaded -= __instance.OnActivePlanetFactoryLoaded;

            // 1. First login and game is not loaded yet, but since client is still syncing, it's ok to resume
            // 2. In game arrive to another planet, after factory model is loaded we can resume
            Multiplayer.Session.Network.PacketProcessor.EnablePacketProcessing = true;
            Log.Info("Resume PacketProcessor (OnActivePlanetFactoryLoaded)");

            // Get the recieved bytes from the remote server that we will import
            if (Multiplayer.Session.Planets.PendingTerrainData.TryGetValue(planet.id, out var terrainBytes))
            {
                // Apply terrian changes, code from PlanetFactory.FlattenTerrainReform()
                if (planet.type != EPlanetType.Gas)
                {
                    planet.data.modData = terrainBytes;
                    for (var i = 0; i < planet.dirtyFlags.Length; i++)
                    {
                        planet.dirtyFlags[i] = true;
                    }
                    planet.landPercentDirty = true;
                    try
                    {
                        planet.UpdateDirtyMeshes();
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                    }
                }
                Multiplayer.Session.Planets.PendingTerrainData.Remove(planet.id);
            }
            Task.Run(() =>
            {
                // This is to fix GS2 that sometimes client mecha will be stuck on ground
                RefreshMissingMeshes();
                Thread.Sleep(1000);
                RefreshMissingMeshes();
            });

            Multiplayer.Session.Trashes.Refresh();
            Multiplayer.Session.Combat.OnFactoryLoadFinished(planet.factory);
            Multiplayer.Session.Enemies.OnFactoryLoadFinished(planet.factory);

            try
            {
                NebulaModAPI.OnPlanetLoadFinished?.Invoke(planet.id);
            }
            catch (Exception e)
            {
                Log.Error("NebulaModAPI.OnPlanetLoadFinished error:\n" + e);
            }
        }

        // call this here as it would not be called normally on the client, but its needed to set GameMain.data.galacticTransport.stationCursor
        // Arragement() updates galacticTransport.stationCursor
        // galacticTransport.shipRenderer.Update() can then update galacticTransport.shipRenderer.shipCount
        // galacticTransport.shipRenderer.Draw() can then render ships
        GameMain.data.galacticTransport.Arragement();
        return false;
    }

    // this fixes werid planet movements while loading factory data from server
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameData.LateUpdate))]
    public static void LateUpdate_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        if (GameMain.data.localPlanet != null && GameMain.data.localPlanet.factoryLoading &&
            GameMain.data.localPlanet.physics != null)
        {
            GameMain.data.localPlanet.physics.LateUpdate();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameData.SetForNewGame))]
    public static void SetForNewGame_Postfix(GameData __instance)
    {
        //Set starting star and planet to request from the server, except when client has set custom starting planet.
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        //Update player's position before searching for closest planet (GS2: Modeler.ModelingCoroutine)
        __instance.mainPlayer.uPosition = new VectorLF3(Multiplayer.Session.LocalPlayer.Data.UPosition.x,
            Multiplayer.Session.LocalPlayer.Data.UPosition.y, Multiplayer.Session.LocalPlayer.Data.UPosition.z);

        if (Multiplayer.Session.LocalPlayer.Data.LocalPlanetId != -1)
        {
            var planet = __instance.galaxy.PlanetById(Multiplayer.Session.LocalPlayer.Data.LocalPlanetId);
            __instance.ArrivePlanet(planet);
        }
        else if (UIVirtualStarmap_Transpiler.CustomBirthPlanet == -1)
        {
            StarData nearestStar = null;
            PlanetData nearestPlanet = null;
            GameMain.data.GetNearestStarPlanet(ref nearestStar, ref nearestPlanet);
            if (nearestStar == null)
            {
                // We are not in a planetary system and thus do not have a star, return.
                return;
            }

            __instance.ArriveStar(nearestStar);
        }
        else
        {
            var planet = __instance.galaxy.PlanetById(UIVirtualStarmap_Transpiler.CustomBirthPlanet);
            __instance.ArrivePlanet(planet);
        }
    }

    [HarmonyPostfix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(GameData.NewGame))]
    public static void NewGame_Postfix(GameData __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost) return;

        // Overwrite from binaryData in GlobalGameDataResponse
        Multiplayer.Session.State.OverwriteGlobalGameData(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameData.GameTick))]
    public static void GameTick_Postfix(long time)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        Multiplayer.Session.Couriers.GameTick();
        Multiplayer.Session.Belts.GameTick();
        Multiplayer.Session.Combat.GameTick();

        if (Multiplayer.Session.LocalPlayer.IsHost)
        {
            Multiplayer.Session.Launch.CollectProjectile();
            Multiplayer.Session.Statistics.SendBroadcastIfNeeded(time);
            return;
        }

        try
        {
            // Client: Update visual effects that don't affect the production
            Multiplayer.Session.Launch.LaunchProjectile();
            ILSUpdateShipPos(time);
        }
        catch (Exception e)
        {
            _ = e;
#if DEBUG
            Log.Warn(e);
#endif
        }
    }

    private static void ILSUpdateShipPos(long time)
    {
        if (!Multiplayer.Session.IsGameLoaded) return;

        // call StationComponent::InternalTickRemote() from here, see StationComponent_Patch.cs for info
        var timeGene = (int)(time % 60L);
        if (timeGene < 0)
        {
            timeGene += 60;
        }
        var history = GameMain.history;
        var shipSailSpeed = history.logisticShipSailSpeedModified;
        var shipWarpSpeed = !history.logisticShipWarpDrive ? shipSailSpeed : history.logisticShipWarpSpeedModified;
        var shipCarries = history.logisticShipCarries;
        var gameData = GameMain.data;
        var gStationPool = gameData.galacticTransport.stationPool;
        var astroPoses = gameData.galaxy.astrosData;
        var relativePos = gameData.relativePos;
        var relativeRot = gameData.relativeRot;
        var starmap = UIGame.viewMode == EViewMode.Starmap;

        foreach (var stationComponent in GameMain.data.galacticTransport.stationPool)
        {
            if (stationComponent != null && stationComponent.isStellar && stationComponent.planetId > 0)
            {
                var planet = GameMain.galaxy.PlanetById(stationComponent.planetId);
                if (planet == null) continue;

                StationComponent_Transpiler.ILSUpdateShipPos(stationComponent,
                    planet.factory, timeGene, shipSailSpeed, shipWarpSpeed,
                    shipCarries, gStationPool, astroPoses, ref relativePos, ref relativeRot, starmap, null);
            }
        }
    }

    private static void InitLandingPlace(GameData gameData, PlanetData planet)
    {
        var birthPoint = planet.birthPoint;
        var quaternion = Maths.SphericalRotation(birthPoint, 0f);
        gameData.mainPlayer.transform.localPosition = birthPoint;
        gameData.mainPlayer.transform.localRotation = quaternion;
        gameData.mainPlayer.transform.localScale = Vector3.one;
        gameData.mainPlayer.uPosition = (Vector3)planet.uPosition + planet.runtimeRotation * birthPoint;
        gameData.mainPlayer.uRotation = planet.runtimeRotation * quaternion;
        gameData.mainPlayer.uVelocity = VectorLF3.zero;
        gameData.mainPlayer.controller.velocityOnLanding = Vector3.zero;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.LeaveStar))]
    public static void LeaveStar_Prefix()
    {
        //Client should unload all factories once they leave the star system
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        PlanetManager.UnloadAllFactories();
        if (!Multiplayer.Session.IsInLobby)
        {
            Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(Multiplayer.Session.LocalPlayer.Id, -1));
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.ArriveStar))]
    public static void ArriveStar_Prefix(StarData star)
    {
        //Client should unload all factories once they leave the star system
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.IsInLobby || star == null)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(Multiplayer.Session.LocalPlayer.Id, star.id));
        Multiplayer.Session.Network.SendPacket(new ILSArriveStarPlanetRequest(star.id));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.LeavePlanet))]
    public static void LeavePlanet_Prefix()
    {
        //Players should clear the list of drone orders of other players when they leave the planet
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Trashes.Refresh();
            Multiplayer.Session.PowerTowers.ResetAndBroadcast();
            Multiplayer.Session.Enemies.OnLeavePlanet();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.DetermineLocalPlanet))]
    public static bool DetermineLocalPlanet_Prefix(ref bool __result)
    {
        if (UIVirtualStarmap_Transpiler.CustomBirthPlanet == -1 || !Multiplayer.IsActive || Multiplayer.Session.IsGameLoaded)
        {
            return true;
        }
        __result = false;
        return false;
    }

    private static void RefreshMissingMeshes()
    {
        var planetData = GameMain.localPlanet;
        var flag = false;

        if (planetData.meshColliders != null)
        {
            for (var i = 0; i < planetData.meshColliders.Length; i++)
            {
                if (planetData.meshColliders[i] == null || planetData.meshColliders[i].sharedMesh != null)
                {
                    continue;
                }
                planetData.meshColliders[i].sharedMesh = planetData.meshes[i];
                flag = true;
            }
        }

        if (flag)
        {
            Log.Debug("RefreshMissingMeshes");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameData.CreateDysonSphere))]
    public static bool CreateDysonSphere_Prefix(GameData __instance, int starIndex, ref DysonSphere __result)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;


        if ((ulong)starIndex >= (ulong)((long)__instance.galaxy.starCount))
        {
            __result = null;
            return false;
        }
        if (__instance.dysonSpheres[starIndex] != null)
        {
            __result = __instance.dysonSpheres[starIndex];
            return false;
        }

        // Create a dummy dyson sphere and prevent sending packets
        Multiplayer.Session.DysonSpheres.InBlueprint = true;
        __instance.dysonSpheres[starIndex] = new DysonSphere();
        __instance.dysonSpheres[starIndex].Init(__instance, __instance.galaxy.stars[starIndex]);
        __instance.dysonSpheres[starIndex].ResetNew();
        Multiplayer.Session.DysonSpheres.InBlueprint = false;

        if (Multiplayer.Session.DysonSpheres.RequestingIndex == -1)
        {
            // If client is not requesting yet, request the target sphere from server
            Multiplayer.Session.DysonSpheres.RequestDysonSphere(starIndex, false);
        }
        __result = __instance.dysonSpheres[starIndex];
        return false;
    }
}
