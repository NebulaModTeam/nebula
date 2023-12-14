#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
internal class PlayerUpdateLocalStarIdProcessor : PacketProcessor<PlayerUpdateLocalStarId>
{
    private readonly IPlayerManager playerManager;

    public PlayerUpdateLocalStarIdProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = playerManager.GetPlayer(conn);
        if (player != null)
        {
            player.Data.LocalStarId = packet.StarId;
        }
    }
}
