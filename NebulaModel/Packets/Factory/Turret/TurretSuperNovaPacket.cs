namespace NebulaModel.Packets.Factory.Turret;

public class TurretSuperNovaPacket
{
    public TurretSuperNovaPacket() { }

    public TurretSuperNovaPacket(int turretIndex, int brustModeIndex, bool setSuperNova, int planetId)
    {
        TurretIndex = turretIndex;
        BrustModeIndex = brustModeIndex;
        SetSuperNova = setSuperNova;
        PlanetId = planetId;
    }

    public int TurretIndex { get; set; }
    public int BrustModeIndex { get; set; }
    public bool SetSuperNova { get; set; }
    public int PlanetId { get; set; }
}
