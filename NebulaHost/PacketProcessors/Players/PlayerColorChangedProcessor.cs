using LiteNetLib;
using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangedProcessor : IPacketProcessor<PlayerColorChanged>
    {
        private PlayerManager playerManager;

        public PlayerColorChangedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            player.PlayerColor = packet.Color;
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.ReliableUnordered);

            // TODO: Should update the host state immediatly here
        }
    }
}
