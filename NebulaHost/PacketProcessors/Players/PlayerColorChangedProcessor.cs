using LiteNetLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

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
            player.Data.Color = packet.Color;
            //playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.ReliableUnordered);
            playerManager.SendPacketToAllPlayers(packet, DeliveryMethod.ReliableUnordered);

            SimulatedWorld.UpdatePlayerColor(packet.PlayerId, packet.Color);
        }
    }
}
