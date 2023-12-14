namespace NebulaModel.Packets.Factory.Inserter;

public class InserterItemUpdatePacket
{
    public InserterItemUpdatePacket() { }

    public InserterItemUpdatePacket(in InserterComponent inserter, int planetId)
    {
        InserterIndex = inserter.id;
        ItemId = inserter.itemId;
        ItemCount = inserter.itemCount;
        ItemInc = inserter.itemInc;
        StackCount = inserter.stackCount;
        PlanetId = planetId;
    }

    public int InserterIndex { get; }
    public int ItemId { get; }
    public short ItemCount { get; }
    public short ItemInc { get; }
    public int StackCount { get; }
    public int PlanetId { get; }
}
