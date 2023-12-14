namespace NebulaModel.Packets.Factory.Belt;

public class BeltUpdatePutItemOnPacket
{
    public BeltUpdatePutItemOnPacket() { }

    public BeltUpdatePutItemOnPacket(int beltId, int itemId, byte itemCount, byte itemInc, int planetId)
    {
        BeltId = beltId;
        ItemId = itemId;
        ItemCount = itemCount;
        ItemInc = itemInc;
        PlanetId = planetId;
    }

    public int BeltId { get; }
    public int ItemId { get; }
    public byte ItemCount { get; }
    public byte ItemInc { get; }
    public int PlanetId { get; }
}
