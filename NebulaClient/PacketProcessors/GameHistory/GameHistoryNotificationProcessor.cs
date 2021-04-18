using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Processors;
using NebulaWorld.GameDataHistory;

namespace NebulaClient.PacketProcessors.GameHistory
{
    [RegisterPacketProcessor]
    class GameHistoryNotificationProcessor : IPacketProcessor<GameHistoryNotificationPacket>
    {
        public void ProcessPacket(GameHistoryNotificationPacket packet, NebulaConnection conn)
        {
            using (GameDataHistoryManager.IsIncomingRequest.On())
            {
                switch (packet.Event)
                {
                    case GameHistoryEvent.ResumeQueue:
                        Log.Info($"Pausing tech queue");
                        GameMain.history.ResumeTechQueue();
                        break;
                    case GameHistoryEvent.PauseQueue:
                        Log.Info($"Resuming tech queue");
                        GameMain.history.PauseTechQueue();
                        break;
                }
            }
        }
    }
}