using NebulaAPI;
using NebulaModel;
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
        private IPlayerManager playerManager;

        public PlayerColorChangedProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerColorChanged packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
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
