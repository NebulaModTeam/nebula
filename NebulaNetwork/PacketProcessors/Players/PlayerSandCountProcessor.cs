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
        if (IsHost)
        {
            // when receive update request, host UpdateSyncedSandCount and send to other players
            GameMain.mainPlayer.SetSandCount(packet.SandCount);
            return;
        }

        // taken from Player.SetSandCount()
        var sandCount = packet.SandCount;
        var player = GameMain.mainPlayer;

        if (sandCount > 1000000000)
        {
            sandCount = 1000000000;
        }
        var num = sandCount - player.sandCount;
        player.sandCount = sandCount;
        if (num != 0)
        {
            UIRoot.instance.uiGame.OnSandCountChanged(player.sandCount, num);
        }
    }
}
