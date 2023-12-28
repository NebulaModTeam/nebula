namespace NebulaModel.Packets.Factory.Turret;

public class TurretGroupUpdatePacket
{
    public TurretGroupUpdatePacket() { }

    public TurretGroupUpdatePacket(int turretIndex, byte group, int planetId)
    {
        TurretIndex = turretIndex;
        Group = group;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public byte Group { get; set; }
    public int PlanetId { get; set; }
}
