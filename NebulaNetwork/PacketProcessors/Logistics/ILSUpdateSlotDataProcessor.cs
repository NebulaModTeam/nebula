using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;
using NebulaWorld.Logistics;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSUpdateSlotDataProcessor: PacketProcessor<ILSUpdateSlotData>
    {
        public override void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
        {
            ILSShipManager.UpdateSlotData(packet);
        }
    }
}
