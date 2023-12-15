#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class ILSUpdateSlotDataProcessor : PacketProcessor<ILSUpdateSlotData>
{
    protected override void ProcessPacket(ILSUpdateSlotData packet, NebulaConnection conn)
    {
        ILSShipManager.UpdateSlotData(packet);
    }
}
