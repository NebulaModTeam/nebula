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

    public int SiloIndex { get; set; }
    public int ItemCount { get; set; }
    public int ItemInc { get; set; }
    public int PlanetId { get; set; }
}
