using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    internal class ILSUpdateSlotDataProcessor : PacketProcessor<ILSUpdateSlotData>
    {
        public override void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
        {
            Multiplayer.Session.Ships.UpdateSlotData(packet);
        }
    }
}
