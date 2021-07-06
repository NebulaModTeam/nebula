using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Players
{
    // Processes packet sent when player starts or stops warping in order to start or stop the warping effect on said player
    [RegisterPacketProcessor]
    class PlayerUseWarperProcessor : IPacketProcessor<PlayerUseWarper>
    {
        private PlayerManager playerManager;

        public PlayerUseWarperProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(PlayerUseWarper packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                packet.PlayerId = player.Id;
                playerManager.SendPacketToOtherPlayers(packet, player);

                SimulatedWorld.UpdateRemotePlayerWarpState(packet);
            }
        }
    }
}