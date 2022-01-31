using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using System.Collections.Generic;

/*
 * when a client arrives at a star he needs to sync the ILS storages as update events are sent only to corresponding stars
 * and also to sync the belt filters conencted to the ILS
 */
namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSArriveStarPlanetRequestProcessor : PacketProcessor<ILSArriveStarPlanetRequest>
    {
        private readonly IPlayerManager playerManager;

        public ILSArriveStarPlanetRequestProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(ILSArriveStarPlanetRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = playerManager.GetPlayer(conn);
            if (player == null)
            {
                player = playerManager.GetSyncingPlayer(conn);
            }
            if (player != null)
            {
                List<int> stationGId = new List<int>();
                List<int> stationMaxShips = new List<int>();
                List<int> storageLength = new List<int>();
                int arraySize = 0;
                int offset = 0;

                foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if (stationComponent != null && GameMain.galaxy.PlanetById(stationComponent.planetId)?.star.id == packet.StarId)
                    {
                        stationGId.Add(stationComponent.gid);
                        stationMaxShips.Add(stationComponent.workShipDatas.Length);
                        storageLength.Add(stationComponent.storage.Length);
                    }
                }

                if (stationGId.Count > 0)
                {
                    StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

                    for (int i = 0; i < storageLength.Count; i++)
                    {
                        arraySize += storageLength[i];
                    }

                    int[] planetId = new int[arraySize];
                    int[] storageIdx = new int[arraySize];
                    int[] itemId = new int[arraySize];
                    int[] count = new int[arraySize];
                    int[] inc = new int[arraySize];

                    for (int i = 0; i < stationGId.Count; i++)
                    {
                        for (int j = 0; j < storageLength[i]; j++)
                        {
                            planetId[offset + j] = gStationPool[stationGId[i]].planetId;
                            if (gStationPool[stationGId[i]].slots.Length > 0) // collectors dont have a slot for belts
                            {
                                storageIdx[offset + j] = gStationPool[stationGId[i]].slots[j].storageIdx;
                            }
                            itemId[offset + j] = gStationPool[stationGId[i]].storage[j].itemId;
                            count[offset + j] = gStationPool[stationGId[i]].storage[j].count;
                            inc[offset + j] = gStationPool[stationGId[i]].storage[j].inc;
                        }
                        offset += storageLength[i];
                    }

                    player.SendPacket(new ILSArriveStarPlanetResponse(stationGId.ToArray(),
                                                                planetId,
                                                                stationMaxShips.ToArray(),
                                                                storageLength.ToArray(),
                                                                storageIdx,
                                                                itemId,
                                                                count,
                                                                inc));
                }
            }
        }
    }
}
