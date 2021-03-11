using LiteNetLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerAnimationUpdateProcessor : IPacketProcessor<PlayerAnimationUpdate>
    {
        private PlayerManager playerManager;

        public PlayerAnimationUpdateProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerAnimationUpdate packet, NebulaConnection conn)
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
