using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : IPacketProcessor<UpgradeEntityRequest>
    {
        public void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            using (FactoryManager.EventFromClient.On())
            {
                UpgradeEntityRequestManager.UpgradeEntityRequest(packet);
            }
        }
    }
}
