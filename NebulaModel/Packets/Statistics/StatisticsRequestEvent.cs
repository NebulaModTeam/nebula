namespace NebulaModel.Packets.Statistics;

public class StatisticsRequestEvent
{
    public StatisticsRequestEvent() { }

    public StatisticsRequestEvent(StatisticEvent statisticEvent, int astroFilter)
    {
        Event = statisticEvent;
        AstroFilter = astroFilter;
    }

    public StatisticEvent Event { get; set; }
    public int AstroFilter { get; set; }
}

public enum StatisticEvent
{
    WindowOpened = 1,
    WindowClosed = 2,
    AstroFilterChanged = 3
}
