using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.GameHistory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    internal class GameHistoryNotificationProcessor : PacketProcessor<GameHistoryNotificationPacket>
    {
        private readonly IPlayerManager playerManager;

        public GameHistoryNotificationProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(GameHistoryNotificationPacket packet, NebulaConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
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