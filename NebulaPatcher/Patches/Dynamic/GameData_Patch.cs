using HarmonyLib;
using LZ4;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaWorld;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameData), "GetOrCreateFactory")]
    class GameData_Patch
    {
        public static bool Prefix(GameData __instance, PlanetFactory __result, PlanetData planet)
        {
            // We want the original method to run on the host client or in single player games
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            Debug.Log("Called GetOrCreateFactory");

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

            // TODO: Possibly rework this a little bit when we implement production stats to match the indexes host and client side
            // Needs more investigation, as we use PlanetFactory.Import()

            // Import the factory from the given bytes, which will have been gotten or created on the host by the original function
            __instance.factories[__instance.factoryCount] = new PlanetFactory();
            using (MemoryStream ms = new MemoryStream(factoryBytes))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                if(planet.factory == null)
                {
                    __instance.factories[__instance.factoryCount].Import(__instance.factoryCount, __instance, br);
                    planet.factory = __instance.factories[__instance.factoryCount];
                    planet.factoryIndex = __instance.factoryCount;

                    __instance.factoryCount++;
                }
                else
                {
                    __instance.factories[planet.factoryIndex].Import(planet.factoryIndex, __instance, br);
                    planet.factory = __instance.factories[planet.factoryIndex];
                }
            }

            // Assign the factory to the result
            __result = __instance.factories[planet.factoryIndex];

            // Do not run the original method
            return false;
        }
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
            if(planet != null)
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
            if(planet != null)
            {
                if(GameMain.gameTick == 0L && DSPGame.SkipPrologue)
                {
                    GameData_Patch3_Helper.InitLandingPlace(__instance, planet);
                }
                // now set localPlanet and planetId and also send the update to the server/other players (to sync localPlanet.id)
                __instance.localPlanet = planet;
                __instance.mainPlayer.planetId = planet.id;

                var packet = new localPlanetSyncPckt(__instance.localPlanet.id, false);
                packet.playerId = LocalPlayer.PlayerId;
                LocalPlayer.SendPacket(packet);
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
