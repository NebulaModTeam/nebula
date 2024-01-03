#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Routers;

[RegisterPacketProcessor]
internal class StarBroadcastProcessor : PacketProcessor<StarBroadcastPacket>
{
    private readonly IPlayerManager playerManager;

    public StarBroadcastProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = playerManager.GetPlayer(conn);
        if (player == null || packet.PacketObject == null)
        {
            return;
        }
        //Forward packet to other users
        playerManager.SendRawPacketToStar(packet.PacketObject, packet.StarId, conn);
        ((INetworkProvider)Multiplayer.Session.Network).PacketProcessor
            .EnqueuePacketForProcessing(packet.PacketObject, conn);
    }
}
