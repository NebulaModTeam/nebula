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
                List<int> stationPId = new List<int>();
                List<int> stationMaxShips = new List<int>();
                List<int> storageLength = new List<int>();
                List<int> slotLength = new List<int>();
                int arraySizeStorage = 0;
                int arraySizeSlot = 0;
                int offsetStorage = 0;
                int offsetSlot = 0;

                foreach (StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if (stationComponent != null && GameMain.galaxy.PlanetById(stationComponent.planetId)?.star.id == packet.StarId)
                    {
                        stationGId.Add(stationComponent.gid);
                        stationPId.Add(stationComponent.planetId);
                        stationMaxShips.Add(stationComponent.workShipDatas.Length);
                        storageLength.Add(stationComponent.storage.Length);
                        slotLength.Add(stationComponent.slots.Length);
                    }
                }

                if (stationGId.Count > 0)
                {
                    StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

                    for (int i = 0; i < storageLength.Count; i++)
                    {
                        arraySizeStorage += storageLength[i];
                    }
                    for(int i = 0; i < slotLength.Count; i++)
                    {
                        arraySizeSlot += slotLength[i];
                    }

                    int[] storageIdx = new int[arraySizeSlot];

                    int[] itemId = new int[arraySizeStorage];
                    int[] count = new int[arraySizeStorage];
                    int[] inc = new int[arraySizeStorage];

                    for (int i = 0; i < stationGId.Count; i++)
                    {
                        for(int j = 0; j < slotLength[i]; j++)
                        {
                            if (gStationPool[stationGId[i]].slots.Length > 0) // collectors dont have a slot for belts
                            {
                                storageIdx[offsetSlot + j] = gStationPool[stationGId[i]].slots[j].storageIdx;
                            }
                        }
                        offsetSlot += slotLength[i];

                        for (int j = 0; j < storageLength[i]; j++)
                        {
                            itemId[offsetStorage + j] = gStationPool[stationGId[i]].storage[j].itemId;
                            count[offsetStorage + j] = gStationPool[stationGId[i]].storage[j].count;
                            inc[offsetStorage + j] = gStationPool[stationGId[i]].storage[j].inc;
                        }
                        offsetStorage += storageLength[i];
                    }

                    player.SendPacket(new ILSArriveStarPlanetResponse(stationGId.ToArray(),
                                                                stationPId.ToArray(),
                                                                stationMaxShips.ToArray(),
                                                                storageLength.ToArray(),
                                                                storageIdx,
                                                                slotLength.ToArray(),
                                                                itemId,
                                                                count,
                                                                inc));
                }
            }
        }
    }
}
