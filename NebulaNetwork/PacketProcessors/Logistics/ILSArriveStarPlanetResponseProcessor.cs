#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class ILSArriveStarPlanetResponseProcessor : PacketProcessor<ILSArriveStarPlanetResponse>
{
    protected override void ProcessPacket(ILSArriveStarPlanetResponse packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        var gStationPool = GameMain.data.galacticTransport.stationPool;

        var offsetStorage = 0;
        var offsetSlot = 0;
        for (var i = 0; i < packet.StationGId.Length; i++)
        {
            if (packet.StationGId[i] >= gStationPool.Length || gStationPool[packet.StationGId[i]] == null)
            {
                ILSShipManager.CreateFakeStationComponent(packet.StationGId[i], packet.StationPId[i],
                    packet.StationMaxShips[i]);
            }

            var stationComponent = gStationPool[packet.StationGId[i]];
            if (stationComponent.slots == null && !stationComponent.isCollector)
            {
                stationComponent.slots = new SlotData[packet.SlotLength[i]];
            }
            if (stationComponent.storage.Length == 0)
            {
                stationComponent.storage = new StationStore[packet.StorageLength[i]];
            }
            for (var j = 0; j < packet.SlotLength[i]; j++)
            {
                var index = offsetSlot + j;

                if (!stationComponent.isCollector)
                {
                    stationComponent.slots[j].storageIdx = packet.StorageIdx[index];
                }
            }
            offsetSlot += packet.SlotLength[i];
            for (var j = 0; j < packet.StorageLength[i]; j++)
            {
                var index = offsetStorage + j;

                stationComponent.storage[j].itemId = packet.ItemId[index];
                stationComponent.storage[j].count = packet.Count[index];
                stationComponent.storage[j].inc = packet.Inc[index];
            }
            offsetStorage += packet.StorageLength[i];
        }
    }
}
