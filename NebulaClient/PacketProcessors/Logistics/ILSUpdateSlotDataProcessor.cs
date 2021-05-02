using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld.Logistics;

namespace NebulaClient.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSUpdateSlotDataProcessor: IPacketProcessor<ILSUpdateSlotData>
    {
        public void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
        {
            ILSShipManager.UpdateSlotData(packet);
        }
    }
}
