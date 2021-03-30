using NebulaModel.Packets.Logistics;
using NebulaModel.Logger;

namespace NebulaWorld.Logistics
{
    class ILSShipManager
    {
        public static void IdleShipGetToWork(ILSShipData packet)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            PlanetData planetA = GameMain.galaxy.PlanetById(packet.planetA);
            PlanetData planetB = GameMain.galaxy.PlanetById(packet.planetB);

            if(planetA != null && planetB != null)
            {
                Log.Info($"Received ship message (departing): {planetA.displayName} -> {planetB.displayName} transporting {packet.itemCount} of {packet.itemId}");
                Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
            }
        }

        public static void WorkShipBackToIdle(ILSShipData packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Log.Info($"Received ship message (landing): transporting {packet.itemCount} of {packet.itemId}");
            Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
        }
    }
}
