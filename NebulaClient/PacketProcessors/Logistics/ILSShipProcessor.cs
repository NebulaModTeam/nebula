using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipProcessor: IPacketProcessor<ILSShipData>
    {
        public void ProcessPacket(ILSShipData packet, NebulaConnection conn)
        {
            SimulatedWorld.OnILSShipUpdate(packet);
        }
    }
}
