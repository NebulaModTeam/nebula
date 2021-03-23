using HarmonyLib;
using LZ4;
using NebulaModel.Logger;
using NebulaWorld;
using System.IO;
using System.IO.Compression;

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

            // Import the factory from the given bytes, which will have been gotten or created on the host by the original function
            __instance.factories[__instance.factoryCount] = new PlanetFactory();

            using (MemoryStream ms = new MemoryStream(factoryBytes))
            using (LZ4Stream ls = new LZ4Stream(ms, CompressionMode.Decompress))
            using (BufferedStream bs = new BufferedStream(ls, 8192))
            using (BinaryReader br = new BinaryReader(bs))
            {
                __instance.factories[__instance.factoryCount].Import(__instance.factoryCount, __instance, br);
            }

            // Assign the factory to the result
            __result = __instance.factories[__instance.factoryCount];

            // TODO: Possibly rework this a little bit when we implement production stats to match the indexes host and client side
            // Loading factories in a different order will cause the indexes to mismatch
            planet.factory = __result;
            planet.factoryIndex = __instance.factoryCount;

            // Bump the factory count up and clear the flag
            __instance.factoryCount++;
            planet.factoryLoading = false;

            // Do not run the original method
            return false;
        }
    }
}
