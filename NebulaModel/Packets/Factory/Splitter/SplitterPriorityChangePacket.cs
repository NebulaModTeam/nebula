namespace NebulaModel.Packets.Factory.Splitter
{
    public class SplitterPriorityChangePacket
    {
        public int SplitterIndex { get; set; }
        public int Slot { get; set; }
        public bool IsPriority { get; set; }
        public int Filter { get; set; }
        public int FactoryIndex { get; set; }

        public SplitterPriorityChangePacket() { }

        public SplitterPriorityChangePacket(int splitterIndex, int slot, bool isPriority, int filter, int factoryIndex)
        {
            SplitterIndex = splitterIndex;
            Slot = slot;
            IsPriority = isPriority;
            Filter = filter;
            FactoryIndex = factoryIndex;
        }
    }
}
