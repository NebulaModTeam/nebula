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
                Log.Info($"Received ship message (departing): {planetA.displayName} -> {planetB.displayName} transporting {packet.itemCount} of {packet.itemId} and index is {packet.origShipIndex}");
                Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
            }
        }

        public static void WorkShipBackToIdle(ILSShipData packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            Log.Info($"Received ship message (landing): transporting {packet.itemCount} of {packet.itemId} and index is {packet.origShipIndex}");
            Log.Info($"Array Length is: {GameMain.data.galacticTransport.stationPool.Length} and there is also: {GameMain.data.galacticTransport.stationCapacity}");
        }

        public static void AddTakeItem(ILSShipItems packet)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return;
            }

            foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
            {
                if(stationComponent != null && stationComponent.gid == packet.stationGID)
                {
                    PlanetData pData = GameMain.galaxy.PlanetById(stationComponent.planetId);
                    if(pData?.factory?.transport != null)
                    {
                        foreach(StationComponent stationComponentPlanet in pData.factory.transport.stationPool)
                        {
                            if(stationComponentPlanet != null && stationComponentPlanet.gid == stationComponent.gid)
                            {
                                if (packet.AddItem)
                                {
                                    Log.Info($"Calling AddItem() with item {packet.itemId} and amount {packet.itemCount}");
                                    stationComponentPlanet.AddItem(packet.itemId, packet.itemCount);
                                }
                                else
                                {
                                    Log.Info($"Calling TakeItem() with item {packet.itemId} and amount {packet.itemCount}");
                                    int itemId = packet.itemId;
                                    int itemCount = packet.itemCount;
                                    stationComponentPlanet.TakeItem(ref itemId, ref itemCount);
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}
