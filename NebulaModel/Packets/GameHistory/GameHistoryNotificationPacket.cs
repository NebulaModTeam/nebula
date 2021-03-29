namespace NebulaModel.Packets.GameHistory
{
    public class GameHistoryNotificationPacket
    {
        public GameHistoryEvent Event { get; set; }

        public GameHistoryNotificationPacket() { }
        public GameHistoryNotificationPacket(GameHistoryEvent Event)
        {
            this.Event = Event;
        }
    }

    public enum GameHistoryEvent
    {
        PauseQueue = 1,
        ResumeQueue = 2
    }
}
