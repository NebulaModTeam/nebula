namespace NebulaModel.Packets.Factory.Belt;

public class BeltSignalNumberPacket
{
    public BeltSignalNumberPacket() { }

    public BeltSignalNumberPacket(int entityId, float number, int planetId)
    {
        EntityId = entityId;
        Number = number;
        PlanetId = planetId;
    }

    public int EntityId { get; }
    public float Number { get; }
    public int PlanetId { get; }
}
