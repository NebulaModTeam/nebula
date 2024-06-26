#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Routers;

[RegisterPacketProcessor]
internal class StarBroadcastProcessor : PacketProcessor<StarBroadcastPacket>
{
    protected override void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
    {
        //Forward packet to other users if we're the host
        if (IsHost)
            Multiplayer.Session.Server.SendToMatching(packet, p =>
                p.Data.LocalStarId == packet.StarId &&
                !p.Connection.Equals(conn)
            );

        //Forward packet data to be processed
        Multiplayer.Session.Network.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
    }
}
