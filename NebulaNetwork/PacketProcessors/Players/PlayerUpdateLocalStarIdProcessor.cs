#region

using NebulaAPI.Extensions;
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
    public PlayerUpdateLocalStarIdProcessor()
    {
    }

    protected override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = Players.Connected().GetPlayer(conn);
        if (player != null)
        {
            player.Data.LocalStarId = packet.StarId;
        }
    }
}
