using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipItemsProcessor : PacketProcessor<ILSShipItems>
    {
        public override void ProcessPacket(ILSShipItems packet, NebulaConnection conn)
        {
            SimulatedWorld.OnILSShipItemsUpdate(packet);
        }
    }
}
