namespace NebulaModel.Packets.GameHistory;

public class GameHistoryNotificationPacket
{
    public GameHistoryNotificationPacket() { }

    public GameHistoryNotificationPacket(GameHistoryEvent Event)
    {
        this.Event = Event;
    }

    public GameHistoryEvent Event { get; set; }
}

public enum GameHistoryEvent
{
    PauseQueue = 1,
    ResumeQueue = 2,
    OneKeyUnlock = 3
}
