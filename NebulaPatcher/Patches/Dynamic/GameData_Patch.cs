using HarmonyLib;
using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Players;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData))]
    class GameData_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.Update))]
        public static void Update_Prefix()
        {
            if (!Multiplayer.IsActive || !Multiplayer.Session.IsGameLoaded)
                return;

            Multiplayer.Session.World.RenderPlayerNameTagsInGame();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.GetOrCreateFactory))]
        public static bool GetOrCreateFactory_Prefix(GameData __instance, PlanetFactory __result, PlanetData planet)
        {
            // We want the original method to run on the host client or in single player games
            if (!Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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
                int factoryIndex = GameMain.data.factoryCount++;
                // Import the factory from the given bytes, which will have been gotten or created on the host by the original function
                __instance.factories[factoryIndex] = new PlanetFactory();

                if (planet.factory == null)
                {
                    __instance.factories[factoryIndex].Import(factoryIndex, __instance, reader.BinaryReader);
                    planet.factory = __instance.factories[factoryIndex];
                    planet.factoryIndex = factoryIndex;
                }
                else
                {
                    __instance.factories[planet.factoryIndex].Import(planet.factoryIndex, __instance, reader.BinaryReader);
                    planet.factory = __instance.factories[planet.factoryIndex];
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

            if (!Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.OnActivePlanetFactoryLoaded))]
        public static bool OnActivePlanetFactoryLoaded_Prefix(GameData __instance, PlanetData planet)
        {
            // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information

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
                // now set localPlanet and planetId
                GameMain.data.localPlanet = planet;
                __instance.mainPlayer.planetId = planet.id;

                planet.onFactoryLoaded -= __instance.OnActivePlanetFactoryLoaded;
            }
            // sync station storages and slot filter for belt i/o
            // do this once the factory is loaded so the processor has access to PlanetData.factory.transport.stationPool
            Multiplayer.Session.Network.SendPacket(new ILSArriveStarPlanetRequest(0, planet.id));

            // call this here as it would not be called normally on the client, but its needed to set GameMain.data.galacticTransport.stationCursor
            // Arragement() updates galacticTransport.stationCursor
            // galacticTransport.shipRenderer.Update() can then update galacticTransport.shipRenderer.shipCount
            // galacticTransport.shipRenderer.Draw() can then render ships
            GameMain.data.galacticTransport.Arragement();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.SetForNewGame))]
        public static void SetForNewGame_Postfix(GameData __instance)
        {
            //Set starting star and planet to request from the server
            if (Multiplayer.IsActive && !((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
            {
                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).Data.LocalPlanetId != -1)
                {
                    PlanetData planet = __instance.galaxy.PlanetById(((LocalPlayer)Multiplayer.Session.LocalPlayer).Data.LocalPlanetId);
                    __instance.ArrivePlanet(planet);
                }
                else
                {
                    StarData nearestStar = null;
                    PlanetData nearestPlanet = null;
                    //Update player's position before searching for closest star
                    __instance.mainPlayer.uPosition = new VectorLF3(((LocalPlayer)Multiplayer.Session.LocalPlayer).Data.UPosition.x, ((LocalPlayer)Multiplayer.Session.LocalPlayer).Data.UPosition.y, ((LocalPlayer)Multiplayer.Session.LocalPlayer).Data.UPosition.z);
                    GameMain.data.GetNearestStarPlanet(ref nearestStar, ref nearestPlanet);

                    if (nearestStar == null)
                    {
                        // We are not in a planetary system and thus do not have a star, return.
                        return;
                    }

                    __instance.ArriveStar(nearestStar);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameData.GameTick))]
        public static void GameTick_Postfix(GameData __instance, long time)
        {
            if (!Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
            {
                if (Multiplayer.IsActive)
                {
                    Multiplayer.Session.StationsUI.DecreaseCooldown();
                }
                return;
            }
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
                if (stationComponent != null && stationComponent.isStellar)
                {
                    //Debug.Log("enter " + stationComponent.gid + " (" + GameMain.galaxy.PlanetById(stationComponent.planetId).displayName + ")");
                    StationComponent_Transpiler.ILSUpdateShipPos(stationComponent, timeGene, dt, shipSailSpeed, shipWarpSpeed, shipCarries, gStationPool, astroPoses, relativePos, relativeRot, starmap, null);
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
            if (Multiplayer.IsActive && !((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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
                Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(-1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameData.ArriveStar))]
        public static void ArriveStar_Prefix(StarData star)
        {
            //Client should unload all factories once they leave the star system
            if (Multiplayer.IsActive && !((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost && star != null)
            {
                Multiplayer.Session.Network.SendPacket(new PlayerUpdateLocalStarId(star.id));
                Multiplayer.Session.Network.SendPacket(new ILSArriveStarPlanetRequest(star.id, 0));
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
    }
}
