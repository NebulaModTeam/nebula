using LiteNetLib;
using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : IPacketProcessor<PlayerMovement>
    {
        private PlayerManager playerManager;

        public PlayerMovementProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerMovement packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            packet.PlayerId = player.Id;
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);

            // TODO: Should update the host state immediatly here
        }
    }
}
