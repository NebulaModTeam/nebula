using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSArriveStarPlanetResponseProcessor: IPacketProcessor<ILSArriveStarPlanetResponse>
    {
        public void ProcessPacket(ILSArriveStarPlanetResponse packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = null;
            if (packet.planet == 0) // arrive at solar system
            {
                gStationPool = GameMain.data.galacticTransport.stationPool;
            }
            else // arrive at planet
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.planet);
                if(pData?.factory?.transport != null)
                {
                    gStationPool = pData.factory.transport.stationPool;
                }
                else
                {
                    return;
                }
            }

            int offset = 0;
            for(int i = 0; i < packet.stationGId.Length; i++)
            {
                if(packet.stationGId[i] >= gStationPool.Length || gStationPool[packet.stationGId[i]] == null)
                {
                    ILSShipManager.CreateFakeStationComponent(packet.stationGId[i], packet.planetId[i]);
                }

                StationComponent stationComponent = gStationPool[packet.stationGId[i]];
                if (stationComponent.slots == null && !stationComponent.isCollector)
                {
                    stationComponent.slots = new SlotData[packet.storageLength[i]];
                }
                if (stationComponent.storage == null)
                {
                    stationComponent.storage = new StationStore[packet.storageLength[i]];
                }
                for (int j = 0; j < packet.storageLength[i]; j++)
                {
                    int index = offset + j;

                    if (!stationComponent.isCollector)
                    {
                        stationComponent.slots[j].storageIdx = packet.storageIdx[index];
                    }
                    stationComponent.storage[j].itemId = packet.itemId[index];
                    stationComponent.storage[j].count = packet.count[index];
                    stationComponent.storage[j].localOrder = packet.localOrder[index];
                    stationComponent.storage[j].remoteOrder = packet.remoteOrder[index];
                    stationComponent.storage[j].max = packet.max[index];
                    stationComponent.storage[j].localLogic = (ELogisticStorage)packet.localLogic[index];
                    stationComponent.storage[j].remoteLogic = (ELogisticStorage)packet.remoteLogic[index];
                }
                offset += packet.storageLength[i];
            }
        }
    }
}
