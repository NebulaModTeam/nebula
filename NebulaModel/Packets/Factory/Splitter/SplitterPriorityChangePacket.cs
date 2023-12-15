namespace NebulaModel.Packets.Factory.Splitter;

public class SplitterPriorityChangePacket
{
    public SplitterPriorityChangePacket() { }

    public SplitterPriorityChangePacket(int splitterIndex, int slot, bool isPriority, int filter, int planetId)
    {
        SplitterIndex = splitterIndex;
        Slot = slot;
        IsPriority = isPriority;
        Filter = filter;
        PlanetId = planetId;
    }

    public int SplitterIndex { get; set; }
    public int Slot { get; set; }
    public bool IsPriority { get; set; }
    public int Filter { get; set; }
    public int PlanetId { get; set; }
}
