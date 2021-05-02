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
            if (packet.Planet == 0) // arrive at solar system
            {
                gStationPool = GameMain.data.galacticTransport.stationPool;
            }
            else // arrive at planet
            {
                PlanetData pData = GameMain.galaxy.PlanetById(packet.Planet);
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
            for(int i = 0; i < packet.StationGId.Length; i++)
            {
                if(packet.StationGId[i] >= gStationPool.Length || gStationPool[packet.StationGId[i]] == null)
                {
                    ILSShipManager.CreateFakeStationComponent(packet.StationGId[i], packet.PlanetId[i]);
                }

                StationComponent stationComponent = gStationPool[packet.StationGId[i]];
                if (stationComponent.slots == null && !stationComponent.isCollector)
                {
                    stationComponent.slots = new SlotData[packet.StorageLength[i]];
                }
                if (stationComponent.storage == null)
                {
                    stationComponent.storage = new StationStore[packet.StorageLength[i]];
                }
                for (int j = 0; j < packet.StorageLength[i]; j++)
                {
                    int index = offset + j;

                    if (!stationComponent.isCollector)
                    {
                        stationComponent.slots[j].storageIdx = packet.StorageIdx[index];
                    }
                    stationComponent.storage[j].itemId = packet.ItemId[index];
                    stationComponent.storage[j].count = packet.Count[index];
                    stationComponent.storage[j].localOrder = packet.LocalOrder[index];
                    stationComponent.storage[j].remoteOrder = packet.RemoteOrder[index];
                    stationComponent.storage[j].max = packet.Max[index];
                    stationComponent.storage[j].localLogic = (ELogisticStorage)packet.LocalLogic[index];
                    stationComponent.storage[j].remoteLogic = (ELogisticStorage)packet.RemoteLogic[index];
                }
                offset += packet.StorageLength[i];
            }
        }
    }
}
