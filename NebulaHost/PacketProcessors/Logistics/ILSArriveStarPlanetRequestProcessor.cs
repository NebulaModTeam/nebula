using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;

/*
 * when a client arrives at a star he needs to sync the ILS storages to give a feeling of living planet factories
 * and also to sync the belt filters conencted to the ILS
 */
namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSArriveStarPlanetRequestProcessor: IPacketProcessor<ILSArriveStarPlanetRequest>
    {
        private PlayerManager playerManager;
        public ILSArriveStarPlanetRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSArriveStarPlanetRequest packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if(player != null)
            {
                List<int> stationGId = new List<int>();
                List<int> storageLength = new List<int>();
                int arraySize = 0;
                int offset = 0;
                if(packet.PlanetId == 0) // arrive at solar system
                {
                    foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                    {
                        if (stationComponent != null && GameMain.galaxy.PlanetById(stationComponent.planetId)?.star.id == packet.StarId)
                        {
                            stationGId.Add(stationComponent.gid);
                            storageLength.Add(stationComponent.storage.Length);
                        }
                    }
                }
                else // arrive at planet
                {
                    PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetId);
                    if(pData?.factory?.transport != null)
                    {
                        foreach(StationComponent stationComponent in pData.factory.transport.stationPool)
                        {
                            if (stationComponent != null)
                            {
                                stationGId.Add(stationComponent.gid);
                                storageLength.Add(stationComponent.storage.Length);
                            }
                        }
                    }
                }

                if(stationGId.Count > 0)
                {
                    StationComponent[] gStationPool = null;
                    if(packet.PlanetId == 0) // arrive at solar system
                    {
                        gStationPool = GameMain.data.galacticTransport.stationPool;
                    }
                    else // arrive at planet
                    {
                        PlanetData pData = GameMain.galaxy.PlanetById(packet.PlanetId);
                        gStationPool = pData.factory.transport.stationPool;
                    }

                    for(int i = 0; i < storageLength.Count; i++)
                    {
                        arraySize += storageLength[i];
                    }

                    int[] planetId = new int[arraySize];
                    int[] storageIdx = new int[arraySize];
                    int[] itemId = new int[arraySize];
                    int[] count = new int[arraySize];
                    int[] localOrder = new int[arraySize];
                    int[] remoteOrder = new int[arraySize];
                    int[] max = new int[arraySize];
                    int[] localLogic = new int[arraySize];
                    int[] remoteLogic = new int[arraySize];

                    for (int i = 0; i < stationGId.Count; i++)
                    {
                        for(int j = 0; j < storageLength[i]; j++)
                        {
                            planetId[offset + j] = gStationPool[stationGId[i]].planetId;
                            if (gStationPool[stationGId[i]].slots.Length > 0) // collectors dont have a slot for belts
                            {
                                storageIdx[offset + j] = gStationPool[stationGId[i]].slots[j].storageIdx;
                            }
                            itemId[offset + j] = gStationPool[stationGId[i]].storage[j].itemId;
                            count[offset + j] = gStationPool[stationGId[i]].storage[j].count;
                            localOrder[offset + j] = gStationPool[stationGId[i]].storage[j].localOrder;
                            remoteOrder[offset + j] = gStationPool[stationGId[i]].storage[j].remoteOrder;
                            max[offset + j] = gStationPool[stationGId[i]].storage[j].max;
                            localLogic[offset + j] = (int)gStationPool[stationGId[i]].storage[j].localLogic;
                            remoteLogic[offset + j] = (int)gStationPool[stationGId[i]].storage[j].remoteLogic;
                        }
                        offset += storageLength[i];
                    }

                    player.SendPacket(new ILSArriveStarPlanetResponse(stationGId.ToArray(),
                                                                planetId,
                                                                (packet.PlanetId == 0) ? 0 : packet.PlanetId,
                                                                storageLength.ToArray(),
                                                                storageIdx,
                                                                itemId,
                                                                count,
                                                                localOrder,
                                                                remoteOrder,
                                                                max,
                                                                localLogic,
                                                                remoteLogic));
                }
            }
        }
    }
}
