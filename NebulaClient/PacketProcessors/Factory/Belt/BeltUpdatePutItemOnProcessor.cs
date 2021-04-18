using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Belt;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Belt
{
    [RegisterPacketProcessor]
    class BeltUpdatePutItemOnProcessor : IPacketProcessor<BeltUpdatePutItemOnPacket>
    {
        public void ProcessPacket(BeltUpdatePutItemOnPacket packet, NebulaConnection conn)
        {
            if (GameMain.data.factories[packet.FactoryIndex]?.cargoTraffic != null)
            {
                using (FactoryManager.EventFromServer.On())
                {
                    GameMain.data.factories[packet.FactoryIndex].cargoTraffic.PutItemOnBelt(packet.BeltId, packet.ItemId);
                }
            }
        }
    }
}