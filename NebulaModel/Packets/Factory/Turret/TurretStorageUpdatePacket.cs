namespace NebulaModel.Packets.Factory.Turret;

public class TurretStorageUpdatePacket
{
    public TurretStorageUpdatePacket() { }

    public TurretStorageUpdatePacket(in TurretComponent turretComponent, int planetId)
    {
        TurretIndex = turretComponent.id;
        ItemId = turretComponent.itemId;
        ItemCount = turretComponent.itemCount;
        ItemInc = turretComponent.itemInc;
        BulletCount = turretComponent.bulletCount;
        PlanetId = planetId;
    }

    public TurretStorageUpdatePacket(int id, int itemId, short itemCount, short itemInc, int planetId)
    {
        TurretIndex = id;
        ItemId = itemId;
        ItemCount = itemCount;
        ItemInc = itemInc;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public int ItemId { get; set; }
    public short ItemCount { get; set; }
    public short BulletCount { get; set; }
    public short ItemInc { get; set; }
    public int PlanetId { get; set; }
}
