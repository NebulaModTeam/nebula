using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRemoteOrderProcessor : PacketProcessor<ILSRemoteOrderData>
    {
        public override void ProcessPacket(ILSRemoteOrderData packet, NebulaConnection conn)
        {
            SimulatedWorld.OnILSRemoteOrderUpdate(packet);
        }
    }
}
