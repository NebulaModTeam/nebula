using LiteNetLib;
using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

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
            packet.PlayerId = player.Id;
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);

            // TODO: Should update the host state immediatly here
        }
    }
}
