namespace NebulaModel.Packets.Statistics;

public class StatisticsRequestEvent
{
    public StatisticsRequestEvent() { }

    public StatisticsRequestEvent(StatisticEvent Event)
    {
        this.Event = Event;
    }

    public StatisticEvent Event { get; }
}

public enum StatisticEvent
{
    WindowOpened = 1,
    WindowClosed = 2
}
