using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : PacketProcessor<CreatePrebuildsRequest>
    {
        public override void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            using (FactoryManager.IsIncomingRequest.On())
            {
                BuildToolManager.CreatePrebuildsRequest(packet);
            }
        }
    }
}
