using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Planet;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetModelingManager), "RequestLoadPlanetFactory")]
    public class PlanetModelingManager_Patch
    {
        public static bool Prefix(PlanetData planet)
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
    }
    [HarmonyPatch(typeof(PlanetModelingManager), "LoadingPlanetFactoryMain")]
    public class PlanetModelingManager_Patch2
    {
        public static bool Prefix(PlanetData planet)
        {
            if (LocalPlayer.IsMasterClient)
            {
                return true;
            }

            //if we are the client we always need to call GetOrCreateFactory() as this is where we handle the FactoryData received from the server
            // NOTE: currentFactingStage is a private field so i need to use the refstub for now
            if(planet.factory != null && PlanetModelingManager.currentFactingStage == 0)
            {
                GameMain.data.GetOrCreateFactory(planet);
            }
            return true;
        }
    }
}
