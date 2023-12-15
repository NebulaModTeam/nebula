#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSWorkShipBackToIdleProcessor : PacketProcessor<ILSWorkShipBackToIdle>
{
    protected override void ProcessPacket(ILSWorkShipBackToIdle packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        if (IsClient)
        {
            ILSShipManager.WorkShipBackToIdle(packet);
        }
    }
}
