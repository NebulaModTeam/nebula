using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSRemoteOrderProcessor : IPacketProcessor<ILSRemoteOrderData>
    {
        public void ProcessPacket(ILSRemoteOrderData packet, NebulaConnection conn)
        {
            SimulatedWorld.OnILSRemoteOrderUpdate(packet);
        }
    }
}
