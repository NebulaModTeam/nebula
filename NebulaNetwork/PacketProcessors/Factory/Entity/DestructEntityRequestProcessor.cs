using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : PacketProcessor<DestructEntityRequest>
    {
        public override void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            using(FactoryManager.IsIncomingRequest.On())
            {
                DestructEntityRequestManager.DestructEntityRequest(packet);
            }
        }
    }
}
