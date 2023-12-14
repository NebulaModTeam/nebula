namespace NebulaModel.Packets.Factory.Silo;

public class SiloStorageUpdatePacket
{
    public SiloStorageUpdatePacket() { }

    public SiloStorageUpdatePacket(int siloIndex, int itemCount, int itemInc, int planetId)
    {
        SiloIndex = siloIndex;
        ItemCount = itemCount;
        ItemInc = itemInc;
        PlanetId = planetId;
    }

    public int SiloIndex { get; }
    public int ItemCount { get; }
    public int ItemInc { get; }
    public int PlanetId { get; }
}
