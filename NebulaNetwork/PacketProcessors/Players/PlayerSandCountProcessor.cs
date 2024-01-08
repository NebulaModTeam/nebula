#region

using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

#endregion

namespace NebulaNetwork.PacketProcessors.Players;

[RegisterPacketProcessor]
public class PlayerSandCountProcessor : PacketProcessor<PlayerSandCount>
{
    protected override void ProcessPacket(PlayerSandCount packet, NebulaConnection conn)
    {
        if (IsHost && packet.SandChange == 0)
        {
            // when receive update request, host UpdateSyncedSandCount and send to other players
            GameMain.mainPlayer.SetSandCount(packet.SandCount);
            return;
        }

        // taken from Player.SetSandCount()
        var player = GameMain.mainPlayer;
        var originalSandCount = player.sandCount;
        if (packet.SandChange != 0)
        {
            player.sandCount += packet.SandChange;
        }
        else
        {
            var sandCount = packet.SandCount;
            if (sandCount > 1000000000)
            {
                sandCount = 1000000000;
            }
            player.sandCount = sandCount;
        }
        if (player.sandCount != originalSandCount)
        {
            UIRoot.instance.uiGame.OnSandCountChanged(player.sandCount, player.sandCount - originalSandCount);
        }
    }
}
