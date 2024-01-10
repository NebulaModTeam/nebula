namespace NebulaModel.Packets.Factory.Turret;

public class TurretSuperNovaPacket
{
    public TurretSuperNovaPacket() { }

    public TurretSuperNovaPacket(int turretIndex, bool inSuperNova, int planetId)
    {
        TurretIndex = turretIndex;
        InSuperNova = inSuperNova;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public bool InSuperNova { get; set; }
    public int PlanetId { get; set; }
}
