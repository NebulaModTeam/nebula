using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Logistics;
using NebulaPatcher.Patches.Transpilers;
using UnityEngine;
using NebulaModel.Packets.Logistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData))]
    class GameData_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        public static void Update_Prefix()
        {
            if (!SimulatedWorld.Initialized || !SimulatedWorld.IsGameLoaded)
                return;

            SimulatedWorld.RenderPlayerNameTagsInGame();
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetOrCreateFactory")]
        public static bool GetOrCreateFactory_Prefix(GameData __instance, PlanetFactory __result, PlanetData planet)
        {
            // We want the original method to run on the host client or in single player games
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            // Get the recieved bytes from the remote server that we will import
            byte[] factoryBytes;
            if (!LocalPlayer.PendingFactories.TryGetValue(planet.id, out factoryBytes))
            {
                // We messed up, just defer to the default behaviour on the client (will cause desync but not outright crash)
                Log.Error($"PendingFactories did not have value we wanted, factory will not be synced!");
                return true;
            }

            // Take it off the list, as we will process it now
            LocalPlayer.PendingFactories.Remove(planet.id);

            using (BinaryUtils.Reader reader = new BinaryUtils.Reader(factoryBytes))
            {
                GameMain.data.factoryCount = reader.BinaryReader.ReadInt32();
                int factoryIndex = reader.BinaryReader.ReadInt32();
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

            // Do not run the original method
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnActivePlanetLoaded")]
        public static bool OnActivePlanetLoaded_Prefix(GameData __instance, PlanetData planet)
        {
            // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information

            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
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
        [HarmonyPatch("OnActivePlanetFactoryLoaded")]
        public static bool OnActivePlanetFactoryLoaded_Prefix(GameData __instance, PlanetData planet)
        {
            // NOTE: this is part of the weird planet movement fix, see ArrivePlanet() patch for more information

            if (LocalPlayer.IsMasterClient)
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
                AccessTools.Property(typeof(GameData), "localPlanet").SetValue(GameMain.data, planet, null);
                __instance.mainPlayer.planetId = planet.id;

                planet.onFactoryLoaded -= __instance.OnActivePlanetFactoryLoaded;
            }
            // sync station storages and slot filter for belt i/o
            // do this once the factory is loaded so the processor has access to PlanetData.factory.transport.stationPool
            LocalPlayer.SendPacket(new ILSArriveStarPlanetRequest(0, planet.id));

            // call this here as it would not be called normally on the client, but its needed to set GameMain.data.galacticTransport.stationCursor
            // Arragement() updates galacticTransport.stationCursor
            // galacticTransport.shipRenderer.Update() can then update galacticTransport.shipRenderer.shipCount
            // galacticTransport.shipRenderer.Draw() can then render ships
            GameMain.data.galacticTransport.Arragement();
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetForNewGame")]
        public static void SetForNewGame_Postfix(GameData __instance)
        {
            //Set starting star and planet to request from the server
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                if (LocalPlayer.Data.LocalPlanetId != -1)
                {
                    PlanetData planet = __instance.galaxy.PlanetById(LocalPlayer.Data.LocalPlanetId);
                    __instance.ArrivePlanet(planet);
                }
                else
                {
                    StarData nearestStar = null;
                    PlanetData nearestPlanet = null;
                    //Update player's position before searching for closest star
                    __instance.mainPlayer.uPosition = new VectorLF3(LocalPlayer.Data.UPosition.x, LocalPlayer.Data.UPosition.y, LocalPlayer.Data.UPosition.z);
                    GameMain.data.GetNearestStarPlanet(ref nearestStar, ref nearestPlanet);
                    __instance.ArriveStar(nearestStar);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void GameTick_Postfix(GameData __instance, long time)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                if (SimulatedWorld.Initialized)
                {
                    StationUIManager.DecreaseCooldown();
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

            foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if(stationComponent != null && stationComponent.isStellar)
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
        [HarmonyPatch("OnDraw")]
        public static void OnDraw_Postfix()
        {
            if (SimulatedWorld.Initialized)
            {
                SimulatedWorld.OnDronesDraw();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("LeaveStar")]
        public static void LeaveStar_Prefix(GameData __instance)
        {
            //Client should unload all factories once they leave the star system
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                using (ILSShipManager.PatchLockILS.On())
                {
                    for (int i = 0; i < __instance.localStar.planetCount; i++)
                    {
                        if (__instance.localStar.planets != null && __instance.localStar.planets[i] != null)
                        {
                            if (__instance.localStar.planets[i].factory != null)
                            {
                                __instance.localStar.planets[i].factory.Free();
                                __instance.localStar.planets[i].factory = null;
                            }
                        }
                    }
                }
                LocalPlayer.SendPacket(new PlayerUpdateLocalStarId(-1));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("ArriveStar")]
        public static void ArriveStar_Prefix(GameData __instance, StarData star)
        {
            //Client should unload all factories once they leave the star system
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new PlayerUpdateLocalStarId(star.id));
                LocalPlayer.SendPacket(new ILSArriveStarPlanetRequest(star.id, 0));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("LeavePlanet")]
        public static void LeavePlanet_Prefix(GameData __instance)
        {
            //Players should clear the list of drone orders of other players when they leave the planet
            if (SimulatedWorld.Initialized)
            {
                GameMain.mainPlayer.mecha.droneLogic.serving.Clear();
            }
        }
    }
}
