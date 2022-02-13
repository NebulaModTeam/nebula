using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Players;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
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
            if (!Multiplayer.Session.Planets.PendingFactories.TryGetValue(planet.id, out byte[] factoryBytes))
            {
                // We messed up, just defer to the default behaviour on the client (will cause desync but not outright crash)
                Log.Error($"PendingFactories did not have value we wanted, factory will not be synced!");
                return true;
            }

            // Take it off the list, as we will process it now
            Multiplayer.Session.Planets.PendingFactories.Remove(planet.id);

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(factoryBytes))
            {
                int factoryIndex;
                // Import the factory from the given bytes, which will have been gotten or created on the host by the original function
                if (planet.factory == null)
                {
                    factoryIndex = GameMain.data.factoryCount++;
                    planet.factoryIndex = factoryIndex;
                    __instance.factories[factoryIndex] = new PlanetFactory();
                    __instance.factories[factoryIndex].Import(factoryIndex, __instance, reader.BinaryReader);
                    planet.factory = __instance.factories[factoryIndex];
                }
                else
                {
                    factoryIndex = planet.factoryIndex;
                    __instance.factories[factoryIndex].Import(factoryIndex, __instance, reader.BinaryReader);
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

            NebulaModAPI.OnPlanetLoadFinished?.Invoke(planet.id);

            // Do not run the original method
            return false;
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
                    if(planet.physics == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }
                }
            }
            planet.onLoaded -= __instance.OnActivePlanetLoaded;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.OnActivePlanetFactoryLoaded))]
        public static bool OnActivePlanetFactoryLoaded_Prefix(GameData __instance, PlanetData planet)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
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

                // If the game is still loading, we wait till the full loading is completed
                if (Multiplayer.Session.IsGameLoaded)
                {
                    ((NebulaModel.NetworkProvider)Multiplayer.Session.Network).PacketProcessor.Enable = true;
                    Log.Info($"OnActivePlanetLoaded: Resume PacketProcessor");
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
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                if (GameMain.data.localPlanet != null && GameMain.data.localPlanet.factoryLoading && GameMain.data.localPlanet.physics != null)
                {
                    GameMain.data.localPlanet.physics.LateUpdate();
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.SetForNewGame))]
        public static void SetForNewGame_Postfix(GameData __instance)
        {
            //Set starting star and planet to request from the server, except when client has set custom starting planet.
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                if (Multiplayer.Session.LocalPlayer.Data.LocalPlanetId != -1)
                {
                    PlanetData planet = __instance.galaxy.PlanetById(Multiplayer.Session.LocalPlayer.Data.LocalPlanetId);
                    __instance.ArrivePlanet(planet);
                }
                else if(UIVirtualStarmap_Transpiler.customBirthPlanet == -1)
                {
                    StarData nearestStar = null;
                    PlanetData nearestPlanet = null;
                    //Update player's position before searching for closest star
                    __instance.mainPlayer.uPosition = new VectorLF3(Multiplayer.Session.LocalPlayer.Data.UPosition.x, Multiplayer.Session.LocalPlayer.Data.UPosition.y, Multiplayer.Session.LocalPlayer.Data.UPosition.z);
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
                    PlanetData planet = __instance.galaxy.PlanetById(UIVirtualStarmap_Transpiler.customBirthPlanet);
                    __instance.ArrivePlanet(planet);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.GameTick))]
        public static void GameTick_Postfix(GameData __instance, long time)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                if (Multiplayer.IsActive)
                {
                    Multiplayer.Session.StationsUI.DecreaseCooldown();
                    Multiplayer.Session.Launch.CollectProjectile();
                }
                return;
            }
            Multiplayer.Session.Launch.LaunchProjectile();
            // call StationComponent::InternalTickRemote() from here, see StationComponent_Patch.cs for info
            int timeGene = (int)(time % 60L);
            if (timeGene < 0)
            {
                timeGene += 60;
            }
            float dt = 0.016666668f;
            GameHistoryData history = GameMain.history;
            float shipSailSpeed = history.logisticShipSailSpeedModified;
            float shipWarpSpeed = (!history.logisticShipWarpDrive) ? shipSailSpeed : history.logisticShipWarpSpeedModified;
            int shipCarries = history.logisticShipCarries;
            StationComponent[] gStationPool = __instance.galacticTransport.stationPool;
            AstroPose[] astroPoses = __instance.galaxy.astroPoses;
            VectorLF3 relativePos = __instance.relativePos;
            Quaternion relativeRot = __instance.relativeRot;
            bool starmap = UIGame.viewMode == EViewMode.Starmap;

            foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if (stationComponent != null && stationComponent.isStellar && !Multiplayer.Session.IsInLobby)
                {
                    StationComponent_Transpiler.ILSUpdateShipPos(stationComponent, GameMain.galaxy.PlanetById(stationComponent.planetId).factory, timeGene, dt, shipSailSpeed, shipWarpSpeed, shipCarries, gStationPool, astroPoses, relativePos, relativeRot, starmap, null);
                }
            }
        }

        private static void InitLandingPlace(GameData gameData, PlanetData planet)
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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.OnDraw))]
        public static void OnDraw_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.World.OnDronesDraw();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.LeaveStar))]
        public static void LeaveStar_Prefix(GameData __instance)
        {
            //Client should unload all factories once they leave the star system
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
            {
                using (Multiplayer.Session.Ships.PatchLockILS.On())
                {
                    for (int i = 0; i < __instance.localStar.planetCount; i++)
                    {
                        if (__instance.localStar.planets != null && __instance.localStar.planets[i] != null)
                        {
                            if (__instance.localStar.planets[i].factory != null)
                            {
                                __instance.localStar.planets[i].factory.Free();
                                __instance.localStar.planets[i].factory = null;
                                GameMain.data.factoryCount--;
                            }
                        }
                    }
                }
                if (!Multiplayer.Session.IsInLobby)
                {
                    Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(-1));
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.ArriveStar))]
        public static void ArriveStar_Prefix(StarData star)
        {
            //Client should unload all factories once they leave the star system
            if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost && !Multiplayer.Session.IsInLobby && star != null)
            {
                Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(star.id));
                Multiplayer.Session.Network.SendPacket(new ILSArriveStarPlanetRequest(star.id));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.LeavePlanet))]
        public static void LeavePlanet_Prefix()
        {
            //Players should clear the list of drone orders of other players when they leave the planet
            if (Multiplayer.IsActive)
            {
                GameMain.mainPlayer.mecha.droneLogic.serving.Clear();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.DetermineLocalPlanet))]
        public static bool DetermineLocalPlanet_Prefix(GameData __instance, ref bool __result)
        {
            if(UIVirtualStarmap_Transpiler.customBirthPlanet != -1 && (Multiplayer.IsActive && !Multiplayer.Session.IsGameLoaded))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
