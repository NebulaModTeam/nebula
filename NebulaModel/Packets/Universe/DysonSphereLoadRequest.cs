namespace NebulaModel.Packets.Universe;

public class DysonSphereLoadRequest
{
    public DysonSphereLoadRequest() { }

    public DysonSphereLoadRequest(int starIndex, DysonSphereRequestEvent requestEvent)
    {
        StarIndex = starIndex;
        Event = requestEvent;
    }

    public int StarIndex { get; }
    public DysonSphereRequestEvent Event { get; }
}

public enum DysonSphereRequestEvent
{
    List = 1,
    Load = 2,
    Unload = 3
}
