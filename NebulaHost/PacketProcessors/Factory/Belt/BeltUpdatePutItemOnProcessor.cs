using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Belt;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    class BeltUpdatePutItemOnProcessor : IPacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
        {
            using (FactoryManager.EventFromClient.On())
            {
                GameMain.data.factories[packet.FactoryIndex].cargoTraffic.PutItemOnBelt(packet.BeltId, packet.ItemId);
            }
        }
    }
}