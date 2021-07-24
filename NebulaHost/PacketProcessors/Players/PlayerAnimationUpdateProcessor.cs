using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : PacketProcessor<PlayerAnimationUpdate>
    {
        private PlayerManager playerManager;

        public PlayerAnimationUpdateProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public override void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                packet.PlayerId = player.Id;
                playerManager.SendPacketToOtherPlayers(packet, player);

                SimulatedWorld.UpdateRemotePlayerAnimation(packet);
            }
        }
    }
}
