namespace NebulaModel.Packets.Warning;

public class WarningDataRequest
{
    public WarningDataRequest() { }

    public WarningDataRequest(WarningRequestEvent requestEvent)
    {
        Event = requestEvent;
    }

    public WarningRequestEvent Event { get; set; }
}

public enum WarningRequestEvent
{
    Signal = 0,
    Data = 1
}
