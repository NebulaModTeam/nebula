using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerUpdateLocalStarIdProcessor : IPacketProcessor<PlayerUpdateLocalStarId>
    {
        private PlayerManager playerManager;

        public PlayerUpdateLocalStarIdProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                player.Data.LocalStarId = packet.StarId;
            }
        }
    }
}
