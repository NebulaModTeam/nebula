namespace NebulaModel.Packets.Universe
{
    public class DysonSphereLoadRequest
    {
        public int StarIndex { get; set; }
        public DysonSphereRequestEvent Event { get; set; }

        public DysonSphereLoadRequest() { }
        public DysonSphereLoadRequest(int starIndex, DysonSphereRequestEvent requestEvent)
        {
            StarIndex = starIndex;
            Event = requestEvent;
        }
    }

    public enum DysonSphereRequestEvent
    {
        List = 1,
        Load = 2,        
        Unload = 3
    }
}
