#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Logistics;

[RegisterPacketProcessor]
internal class StorageUIProcessor : PacketProcessor<StorageUI>
{
    protected override void ProcessPacket(StorageUI packet, NebulaConnection conn)
    {
        if (IsHost)
        {
            // always update values for host
            packet.ShouldRefund = false;
            Multiplayer.Session.StationsUI.UpdateStorage(packet);

            // broadcast to other clients 
            Server.SendPacketExclude(packet, conn);

            // as we block some methods for the client he must run it once he receives this packet.
            // but only the one issued the request should get items refund
            packet.ShouldRefund = true;
            conn.SendPacket(packet);
        }

        if (IsClient)
        {
            Multiplayer.Session.StationsUI.UpdateStorage(packet);
        }
    }
}
