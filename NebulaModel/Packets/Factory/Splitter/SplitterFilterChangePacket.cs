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

    public int SplitterIndex { get; set; }
    public int ItemId { get; set; }
    public int PlanetId { get; set; }
}
