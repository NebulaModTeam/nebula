using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class StationUIInitialSyncRequestProcessor: IPacketProcessor<StationUIInitialSyncRequest>
    {
        public void ProcessPacket(StationUIInitialSyncRequest packet, NebulaConnection conn)
        {
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            if(packet.stationGId < gStationPool.Length)
            {
                StationComponent stationComponent = null;
                if(packet.planetId == 0)
                {
                    stationComponent = gStationPool[packet.stationGId];
                }
                else
                {
                    PlanetData pData = GameMain.galaxy.PlanetById(packet.planetId);
                    // GId is the id in this case as we look at a PLS
                    if(pData?.factory?.transport != null && pData.factory.transport.stationPool.Length > packet.stationGId)
                    {
                        stationComponent = pData.factory.transport.stationPool[packet.stationGId];
                    }
                }
                if(stationComponent != null)
                {
                    List<int> itemId = new List<int>();
                    List<int> itemCountMax = new List<int>();
                    List<int> itemCount = new List<int>();
                    List<int> localLogic = new List<int>();
                    List<int> remoteLogic = new List<int>();
                    List<int> remoteOrder = new List<int>();
                    for(int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        itemId.Add(stationComponent.storage[i].itemId);
                        itemCountMax.Add(stationComponent.storage[i].max);
                        itemCount.Add(stationComponent.storage[i].count);
                        remoteOrder.Add(stationComponent.storage[i].remoteOrder);
                        switch (stationComponent.storage[i].localLogic)
                        {
                            case (ELogisticStorage.None):
                                localLogic.Add(0);
                                break;
                            case (ELogisticStorage.Supply):
                                localLogic.Add(1);
                                break;
                            case (ELogisticStorage.Demand):
                                localLogic.Add(2);
                                break;
                        }
                        switch (stationComponent.storage[i].remoteLogic)
                        {
                            case (ELogisticStorage.None):
                                remoteLogic.Add(0);
                                break;
                            case (ELogisticStorage.Supply):
                                remoteLogic.Add(1);
                                break;
                            case (ELogisticStorage.Demand):
                                remoteLogic.Add(2);
                                break;
                        }
                    }
                    StationUIInitialSync packet2 = new StationUIInitialSync(packet.stationGId,
                                                                            packet.planetId,
                                                                            stationComponent.tripRangeDrones,
                                                                            stationComponent.tripRangeShips,
                                                                            stationComponent.deliveryDrones,
                                                                            stationComponent.deliveryShips,
                                                                            stationComponent.warpEnableDist,
                                                                            stationComponent.warperNecessary,
                                                                            stationComponent.includeOrbitCollector,
                                                                            stationComponent.energy,
                                                                            stationComponent.energyPerTick,
                                                                            itemId.ToArray(),
                                                                            itemCountMax.ToArray(),
                                                                            itemCount.ToArray(),
                                                                            localLogic.ToArray(),
                                                                            remoteLogic.ToArray(),
                                                                            remoteOrder.ToArray());
                    conn.SendPacket(packet2);
                }
            }
        }
    }
}
