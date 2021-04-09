namespace NebulaModel.Packets.Factory.Splitter
{
    public class SplitterFilterChangePacket
    {
        public int SplitterIndex { get; set; }
        public int ItemId { get; set; }

        public SplitterFilterChangePacket() { }

        public SplitterFilterChangePacket(int splitterIndex, int itemId)
        {
            SplitterIndex = splitterIndex;
            ItemId = itemId;
        }
    }
}
