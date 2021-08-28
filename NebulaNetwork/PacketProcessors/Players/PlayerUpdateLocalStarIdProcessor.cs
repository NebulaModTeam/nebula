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
    class PlayerUpdateLocalStarIdProcessor : PacketProcessor<PlayerUpdateLocalStarId>
    {
        private IPlayerManager playerManager;

        public PlayerUpdateLocalStarIdProcessor()
        {
            playerManager = ((NetworkProvider)Multiplayer.Session.Network).PlayerManager;
        }

        public override void ProcessPacket(PlayerUpdateLocalStarId packet, NebulaConnection conn)
        {
            if (IsClient) return;

            NebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                player.Data.LocalStarId = packet.StarId;
            }
        }
    }
}
