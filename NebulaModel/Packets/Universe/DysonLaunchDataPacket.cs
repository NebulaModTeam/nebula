namespace NebulaModel.Packets.Universe
{
    public class DysonLaunchDataPacket
    {
        public int Count { get; set; }
        public byte[] BinaryData { get; set; }

        public DysonLaunchDataPacket() { }
        public DysonLaunchDataPacket(int num, byte[] data)
        {
            Count = num;
            BinaryData = data;
        }
    }
}
