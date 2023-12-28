namespace NebulaModel.Packets.Factory.Turret;

public class TurretStorageUpdatePacket
{
    public TurretStorageUpdatePacket() { }

    public TurretStorageUpdatePacket(int turretIndex, int itemId, short itemCount, short itemInc, int planetId)
    {
        TurretIndex = turretIndex;
        ItemId = itemId;
        ItemCount = itemCount;
        ItemInc = itemInc;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public int ItemId { get; set; }
    public short ItemCount { get; set; }
    public short ItemInc { get; set; }
    public int PlanetId { get; set; }
}
