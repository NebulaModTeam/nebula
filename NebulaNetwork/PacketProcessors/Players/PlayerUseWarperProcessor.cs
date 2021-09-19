using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    // Processes packet sent when player starts or stops warping in order to start or stop the warping effect on said player
    [RegisterPacketProcessor]
    internal class PlayerUseWarperProcessor : PacketProcessor<PlayerUseWarper>
    {
        private readonly IPlayerManager playerManager;

        public PlayerUseWarperProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerUseWarper packet, NebulaConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    packet.PlayerId = player.Id;
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                Multiplayer.Session.World.UpdateRemotePlayerWarpState(packet);
            }
        }
    }
}