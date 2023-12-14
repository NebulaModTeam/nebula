namespace NebulaModel.Packets.Factory.Ejector;

public class EjectorStorageUpdatePacket
{
    public EjectorStorageUpdatePacket() { }

    public EjectorStorageUpdatePacket(int ejectorIndex, int itemCount, int itemInc, int planetId)
    {
        EjectorIndex = ejectorIndex;
        ItemCount = itemCount;
        ItemInc = itemInc;
        PlanetId = planetId;
    }

    public int EjectorIndex { get; }
    public int ItemCount { get; }
    public int ItemInc { get; }
    public int PlanetId { get; }
}
