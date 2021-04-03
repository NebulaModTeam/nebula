namespace NebulaModel.Packets.Statistics
{
    public class StatisticsRequestEvent
    {
        public StatisticEvent Event { get; set; }

        public StatisticsRequestEvent() { }
        public StatisticsRequestEvent(StatisticEvent Event)
        {
            this.Event = Event;
        }
    }

    public enum StatisticEvent
    {
        WindowOpened = 1,
        WindowClosed = 2
    }
}
