using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Belt;
using NebulaModel.Packets.Processors;

namespace NebulaClient.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    class BeltUpdatePickupItemsProcessor : IPacketProcessor<BeltUpdatePickupItemsPacket>
    {
        public void ProcessPacket(BeltUpdatePickupItemsPacket packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactoryIndex]?.cargoTraffic != null)
            {
                //Iterate though belt updates and remove target items
                for (int i = 0; i < packet.BeltUpdates.Length; i++)
                {
                    CargoTraffic traffic = GameMain.data.factories[packet.FactoryIndex].cargoTraffic;
                    CargoPath cargoPath = traffic.GetCargoPath(traffic.beltPool[packet.BeltUpdates[i].BeltId].segPathId);
                    cargoPath.TryPickItem(packet.BeltUpdates[i].SegId - 4 - 1, 12);
                }
            }
        }
    }
}