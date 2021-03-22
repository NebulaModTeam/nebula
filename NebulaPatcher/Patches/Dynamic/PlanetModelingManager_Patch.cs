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
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            //if we are the client we always need to call GetOrCreateFactory() as this is where we handle the FactoryData received from the server
            // NOTE: currentFactingStage is a private field so i need to use the refstub for now
            //int currentFactingStage = (int)typeof(PlanetModelingManager).GetField("currentFactingStage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(null);
            if (planet.factory != null && PlanetModelingManager.currentFactingStage == 0)
            {
                // now set localPlanet and planetId and also send the update to the server/other players (to sync localPlanet.id)
                //typeof(GameData).GetField("localPlanet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(GameMain.data, planet);
                GameMain.data.localPlanet = planet;
                GameMain.mainPlayer.planetId = planet.id;

                var packet = new localPlanetSyncPckt(GameMain.data.localPlanet.id, false);
                packet.playerId = LocalPlayer.PlayerId;
                LocalPlayer.SendPacket(packet);

                GameMain.data.GetOrCreateFactory(planet);
            }
            return true;
        }
    }
}
