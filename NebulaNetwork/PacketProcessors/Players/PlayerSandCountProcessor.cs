using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerSandCountProcessor: PacketProcessor<PlayerSandCount>
    {
        public override void ProcessPacket(PlayerSandCount packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                // when receive update request, host UpdateSyncedSandCount and send to other players
                GameMain.mainPlayer.SetSandCount(packet.SandCount);
                return;
            }

            // taken from Player.SetSandCount()
            int sandCount = packet.SandCount;
            Player player = GameMain.mainPlayer;

            if (sandCount > 1000000000)
            {
                sandCount = 1000000000;
            }
            int num = sandCount - player.sandCount;
            player.sandCount = sandCount;
            if (num != 0)
            {
                UIRoot.instance.uiGame.OnSandCountChanged(player.sandCount, num);
            }
        }
    }
}
