#region

using System.Collections.Generic;
using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

/*
 * when a client arrives at a star he needs to sync the ILS storages as update events are sent only to corresponding stars
 * and also to sync the belt filters conencted to the ILS
 */
namespace NebulaNetwork.PacketProcessors.Logistics;

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

        var player = playerManager.GetPlayer(conn);
        if (player == null)
        {
            player = playerManager.GetSyncingPlayer(conn);
        }
        if (player != null)
        {
            var stationGId = new List<int>();
            var stationPId = new List<int>();
            var stationMaxShips = new List<int>();
            var storageLength = new List<int>();
            var slotLength = new List<int>();
            var arraySizeStorage = 0;
            var arraySizeSlot = 0;
            var offsetStorage = 0;
            var offsetSlot = 0;

            foreach (var stationComponent in GameMain.data.galacticTransport.stationPool)
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
                var gStationPool = GameMain.data.galacticTransport.stationPool;

                for (var i = 0; i < storageLength.Count; i++)
                {
                    arraySizeStorage += storageLength[i];
                }
                for (var i = 0; i < slotLength.Count; i++)
                {
                    arraySizeSlot += slotLength[i];
                }

                var storageIdx = new int[arraySizeSlot];

                var itemId = new int[arraySizeStorage];
                var count = new int[arraySizeStorage];
                var inc = new int[arraySizeStorage];

                for (var i = 0; i < stationGId.Count; i++)
                {
                    for (var j = 0; j < slotLength[i]; j++)
                    {
                        if (gStationPool[stationGId[i]].slots.Length > 0) // collectors dont have a slot for belts
                        {
                            storageIdx[offsetSlot + j] = gStationPool[stationGId[i]].slots[j].storageIdx;
                        }
                    }
                    offsetSlot += slotLength[i];

                    for (var j = 0; j < storageLength[i]; j++)
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
