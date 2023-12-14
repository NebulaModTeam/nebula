namespace NebulaModel.Packets.Factory.Inserter;

public class InserterFilterUpdatePacket
{
    public InserterFilterUpdatePacket() { }

    public InserterFilterUpdatePacket(int inserterIndex, int itemId, int planetId)
    {
        InserterIndex = inserterIndex;
        ItemId = itemId;
        PlanetId = planetId;
    }

    public int InserterIndex { get; }
    public int ItemId { get; }
    public int PlanetId { get; }
}
