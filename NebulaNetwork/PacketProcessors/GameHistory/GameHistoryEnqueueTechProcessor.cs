using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryEnqueueTechProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    using (Multiplayer.Session.History.IsIncomingRequest.On())
                    {
                        GameMain.history.EnqueueTech(packet.TechId);
                    }
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }
            else
            {
                using (Multiplayer.Session.History.IsIncomingRequest.On())
                {
                    GameMain.history.EnqueueTech(packet.TechId);
                }
            }

        }
    }
}
