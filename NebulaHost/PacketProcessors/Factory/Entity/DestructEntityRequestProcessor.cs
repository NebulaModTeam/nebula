using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : IPacketProcessor<DestructEntityRequest>
    {
        public void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            using(FactoryManager.EventFromClient.On())
            {
                DestructEntityRequestManager.DestructEntityRequest(packet);
            }
        }
    }
}
