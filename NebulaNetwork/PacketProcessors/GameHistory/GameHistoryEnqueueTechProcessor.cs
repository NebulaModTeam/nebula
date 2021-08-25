using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryEnqueueTechProcessor : PacketProcessor<GameHistoryEnqueueTechPacket>
    {
        private IPlayerManager playerManager;

        public GameHistoryEnqueueTechProcessor()
        {
            playerManager = Multiplayer.Session?.Network.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryEnqueueTechPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetPlayer(conn);
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
