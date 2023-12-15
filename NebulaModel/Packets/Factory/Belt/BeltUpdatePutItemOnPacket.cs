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

    public int BeltId { get; set; }
    public int ItemId { get; set; }
    public byte ItemCount { get; set; }
    public byte ItemInc { get; set; }
    public int PlanetId { get; set; }
}
