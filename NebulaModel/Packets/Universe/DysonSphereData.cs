namespace NebulaModel.Packets.Universe
{
    public class DysonSphereData
    {
        public int StarIndex { get; set; }
        public byte[] BinaryData { get; set; }
        public DysonSphereRespondEvent Event { get; set; }

        public DysonSphereData() { }
        public DysonSphereData(int starIndex, byte[] data, DysonSphereRespondEvent respondEvent)
        {
            StarIndex = starIndex;
            BinaryData = data;
            Event = respondEvent;
        }
    }

    public enum DysonSphereRespondEvent
    {
        List = 1,
        Load = 2,        
        Desync = 3
    }
}
