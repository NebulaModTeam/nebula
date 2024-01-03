namespace NebulaModel.Packets.Factory.Turret;

public class TurretBurstUpdatePacket
{
    public TurretBurstUpdatePacket() { }

    public TurretBurstUpdatePacket(int turretIndex, int burstIndex, int planetId)
    {
        TurretIndex = turretIndex;
        BurstIndex = burstIndex;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public int BurstIndex { get; set; }
    public int PlanetId { get; set; }
}
