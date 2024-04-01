#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class StationUIProcessor : PacketProcessor<StationUI>
{
    protected override void ProcessPacket(StationUI packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // always update values for host
            packet.ShouldRefund = false;
            StationUIManager.UpdateStation(ref packet);

            // broadcast to other clients 
            Server.SendPacketExclude(packet, conn);

            // as we block the normal method for the client he must run it once he receives this packet.
            // but only the one issued the request should get items refund
            packet.ShouldRefund = true;
            conn.SendPacket(packet);
        }

        if (IsClient)
        {
            StationUIManager.UpdateStation(ref packet);
        }
    }
}
