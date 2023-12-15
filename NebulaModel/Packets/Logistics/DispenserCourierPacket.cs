namespace NebulaModel.Packets.Logistics;

public class DispenserCourierPacket
{
    public DispenserCourierPacket() { }

    public DispenserCourierPacket(int planetId, int playerId, int dispenserId, int itemId, int itemCount)
    {
        PlanetId = planetId;
        PlayerId = playerId;
        DispenserId = dispenserId;
        ItemId = itemId;
        ItemCount = itemCount;
    }

    public int PlanetId { get; set; }
    public int PlayerId { get; set; }
    public int DispenserId { get; set; }
    public int ItemId { get; set; }
    public int ItemCount { get; set; }
}
