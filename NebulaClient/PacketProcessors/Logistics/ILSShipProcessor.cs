using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipProcessor: PacketProcessor<ILSShipData>
    {
        public override void ProcessPacket(ILSShipData packet, NebulaConnection conn)
        {
            using(FactoryManager.EventFromServer.On())
            {
                SimulatedWorld.OnILSShipUpdate(packet);
            }
        }
    }
}
