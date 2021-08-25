using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;
using NebulaWorld.GameDataHistory;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryNotificationProcessor : PacketProcessor<GameHistoryNotificationPacket>
    {
        private PlayerManager playerManager;

        public GameHistoryNotificationProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryNotificationPacket packet, NebulaConnection conn)
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
                using (Multiplayer.Session.History.IsIncomingRequest.On())
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