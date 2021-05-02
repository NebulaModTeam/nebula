using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipItemsProcessor : IPacketProcessor<ILSShipItems>
    {
        public void ProcessPacket(ILSShipItems packet, NebulaConnection conn)
        {
            SimulatedWorld.OnILSShipItemsUpdate(packet);
        }
    }
}
