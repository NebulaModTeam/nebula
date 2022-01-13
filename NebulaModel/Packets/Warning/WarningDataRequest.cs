namespace NebulaModel.Packets.Warning
{
    public class WarningDataRequest
    {
        public WarningRequestEvent Event { get; set; }

        public WarningDataRequest() { }
        public WarningDataRequest(WarningRequestEvent requestEvent)
        {
            Event = requestEvent;
        }
    }

    public enum WarningRequestEvent
    {
        Signal = 0,
        Data = 1
    }
}
