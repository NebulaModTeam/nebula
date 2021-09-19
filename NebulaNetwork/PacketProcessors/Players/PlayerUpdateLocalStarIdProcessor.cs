using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    internal class PlayerUpdateLocalStarIdProcessor : PacketProcessor<PlayerUpdateLocalStarId>
    {
        private readonly IPlayerManager playerManager;

        public PlayerUpdateLocalStarIdProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                player.Data.LocalStarId = packet.StarId;
            }
        }
    }
}
