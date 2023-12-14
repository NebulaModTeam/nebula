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

    public int SplitterIndex { get; }
    public int Slot { get; }
    public bool IsPriority { get; }
    public int Filter { get; }
    public int PlanetId { get; }
}
