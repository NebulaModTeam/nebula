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
                    StationStore[] storage = stationComponent.storage;
                    int[] itemId = new int[storage.Length];
                    int[] itemCountMax = new int[storage.Length];
                    int[] itemCount = new int[storage.Length];
                    int[] localLogic = new int[storage.Length];
                    int[] remoteLogic = new int[storage.Length];
                    int[] remoteOrder = new int[storage.Length];
                    for(int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        itemId[i] = storage[i].itemId;
                        itemCountMax[i] = storage[i].max;
                        itemCount[i] = storage[i].count;
                        localLogic[i] = (int)storage[i].localLogic;
                        remoteLogic[i] = (int)storage[i].remoteLogic;
                        remoteOrder[i] = storage[i].remoteOrder;
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
                                                                            itemId,
                                                                            itemCountMax,
                                                                            itemCount,
                                                                            localLogic,
                                                                            remoteLogic,
                                                                            remoteOrder);
                    conn.SendPacket(packet2);
                }
            }
        }
    }
}
