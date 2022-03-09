using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSArriveStarPlanetResponseProcessor : PacketProcessor<ILSArriveStarPlanetResponse>
    {
        public override void ProcessPacket(ILSArriveStarPlanetResponse packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                return;
            }

            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;

            int offsetStorage = 0;
            int offsetSlot = 0;
            for (int i = 0; i < packet.StationGId.Length; i++)
            {
                if (packet.StationGId[i] >= gStationPool.Length || gStationPool[packet.StationGId[i]] == null)
                {
                    Multiplayer.Session.Ships.CreateFakeStationComponent(packet.StationGId[i], packet.StationPId[i], packet.StationMaxShips[i]);
                }

                StationComponent stationComponent = gStationPool[packet.StationGId[i]];
                if (stationComponent.slots == null && !stationComponent.isCollector)
                {
                    stationComponent.slots = new SlotData[packet.SlotLength[i]];
                }
                if (stationComponent.storage == null)
                {
                    stationComponent.storage = new StationStore[packet.StorageLength[i]];
                }
                for (int j = 0; j < packet.SlotLength[i]; j++)
                {
                    int index = offsetSlot + j;

                    if (!stationComponent.isCollector)
                    {
                        stationComponent.slots[j].storageIdx = packet.StorageIdx[index];
                    }
                }
                offsetSlot += packet.SlotLength[i];
                for(int j = 0; j < packet.StorageLength[i]; j++)
                {
                    int index = offsetStorage + j;

                    stationComponent.storage[j].itemId = packet.ItemId[index];
                    stationComponent.storage[j].count = packet.Count[index];
                    stationComponent.storage[j].inc = packet.Inc[index];
                }
                offsetStorage += packet.StorageLength[i];
            }
        }
    }
}
