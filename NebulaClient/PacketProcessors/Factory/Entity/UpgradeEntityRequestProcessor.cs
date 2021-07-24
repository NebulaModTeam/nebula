using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : PacketProcessor<UpgradeEntityRequest>
    {
        public override void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            using(FactoryManager.EventFromServer.On())
            {
                UpgradeEntityRequestManager.UpgradeEntityRequest(packet);
            }
        }
    }
}
