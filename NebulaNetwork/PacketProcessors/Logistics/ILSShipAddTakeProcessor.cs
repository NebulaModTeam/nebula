#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSShipAddTakeProcessor : PacketProcessor<ILSShipAddTake>
{
    protected override void ProcessPacket(ILSShipAddTake packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        ILSShipManager.AddTakeItem(packet);
    }
}
