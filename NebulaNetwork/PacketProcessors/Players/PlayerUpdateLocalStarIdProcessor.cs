using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerUpdateLocalStarIdProcessor : PacketProcessor<PlayerUpdateLocalStarId>
    {
        private PlayerManager playerManager;

        public PlayerUpdateLocalStarIdProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
        {
            if (IsClient) return;

            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                player.Data.LocalStarId = packet.StarId;
            }
        }
    }
}
