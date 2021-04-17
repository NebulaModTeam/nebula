using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;
using UnityEngine;

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
                StationComponent stationComponent = gStationPool[packet.stationGId];
                if(stationComponent != null)
                {
                    List<int> itemId = new List<int>();
                    List<int> itemCountMax = new List<int>();
                    List<int> localLogic = new List<int>();
                    List<int> remoteLogic = new List<int>();
                    for(int i = 0; i < stationComponent.storage.Length; i++)
                    {
                        itemId.Add(stationComponent.storage[i].itemId);
                        itemCountMax.Add(stationComponent.storage[i].max);
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
                                                                            stationComponent.tripRangeDrones,
                                                                            stationComponent.tripRangeShips,
                                                                            stationComponent.deliveryDrones,
                                                                            stationComponent.deliveryShips,
                                                                            stationComponent.warpEnableDist,
                                                                            stationComponent.warperNecessary,
                                                                            stationComponent.includeOrbitCollector,
                                                                            itemId.ToArray(),
                                                                            itemCountMax.ToArray(),
                                                                            localLogic.ToArray(),
                                                                            remoteLogic.ToArray());
                    conn.SendPacket(packet2);
                    Debug.Log("send syncing packet " + packet.stationGId);
                }
            }
        }
    }
}
