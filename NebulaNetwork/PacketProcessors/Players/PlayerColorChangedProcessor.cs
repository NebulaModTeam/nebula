using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerColorChangedProcessor : PacketProcessor<PlayerColorChanged>
    {
        private PlayerManager playerManager;

        public PlayerColorChangedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    player.Data.MechaColor = packet.Color;
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                Multiplayer.Session.World.UpdatePlayerColor(packet.PlayerId, packet.Color);
            }
        }
    }
}
