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

    public int InserterIndex { get; set; }
    public int ItemId { get; set; }
    public short ItemCount { get; set; }
    public short ItemInc { get; set; }
    public int StackCount { get; set; }
    public int PlanetId { get; set; }
}
