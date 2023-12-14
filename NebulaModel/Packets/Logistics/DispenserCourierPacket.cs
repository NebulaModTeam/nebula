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

    public int PlanetId { get; }
    public int PlayerId { get; }
    public int DispenserId { get; }
    public int ItemId { get; }
    public int ItemCount { get; }
}
