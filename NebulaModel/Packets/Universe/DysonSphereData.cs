namespace NebulaModel.Packets.Universe
{
    public class DysonSphereData
    {
        public int StarIndex { get; set; }
        public byte[] BinaryData { get; set; }

        public DysonSphereData() { }
        public DysonSphereData(int starIndex, byte[] data)
        {
            this.StarIndex = starIndex;
            this.BinaryData = data;
        }
    }
}
