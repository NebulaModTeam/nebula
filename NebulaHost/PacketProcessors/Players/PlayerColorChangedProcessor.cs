using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangedProcessor : PacketProcessor<PlayerColorChanged>
    {
        private PlayerManager playerManager;

        public PlayerColorChangedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public override void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player == null)
            {
                Log.Info($"Received PlayerColorChanged packet from unknown player {packet.PlayerId}");
                return;
            }

            player.Data.MechaColor = packet.Color;
            playerManager.SendPacketToOtherPlayers(packet, player);

            SimulatedWorld.UpdatePlayerColor(packet.PlayerId, packet.Color);
        }
    }
}
