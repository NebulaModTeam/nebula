#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
public class ILSWorkShipBackToIdleProcessor : PacketProcessor<ILSWorkShipBackToIdle>
{
    public override void ProcessPacket(ILSWorkShipBackToIdle packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            return;
        }

        if (IsClient)
        {
            Multiplayer.Session.Ships.WorkShipBackToIdle(packet);
        }
    }
}
