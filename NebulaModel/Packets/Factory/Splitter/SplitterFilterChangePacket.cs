namespace NebulaModel.Packets.Factory.Splitter;

public class SplitterFilterChangePacket
{
    public SplitterFilterChangePacket() { }

    public SplitterFilterChangePacket(int splitterIndex, int itemId, int planetId)
    {
        SplitterIndex = splitterIndex;
        ItemId = itemId;
        PlanetId = planetId;
    }

    public int SplitterIndex { get; }
    public int ItemId { get; }
    public int PlanetId { get; }
}
