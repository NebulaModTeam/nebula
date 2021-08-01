using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryNotificationProcessor : PacketProcessor<GameHistoryNotificationPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryNotificationProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(GameHistoryNotificationPacket packet, NetworkConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (GameDataHistoryManager.IsIncomingRequest.On())
                {
                    switch (packet.Event)
                    {
                        case GameHistoryEvent.ResumeQueue:
                            GameMain.history.ResumeTechQueue();
                            break;
                        case GameHistoryEvent.PauseQueue:
                            GameMain.history.PauseTechQueue();
                            break;
                    }
                }
            }
        }
    }
}