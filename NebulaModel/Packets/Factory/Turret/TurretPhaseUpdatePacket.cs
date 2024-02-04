namespace NebulaModel.Packets.Factory.Turret;

public class TurretPhaseUpdatePacket
{
    public TurretPhaseUpdatePacket() { }

    public TurretPhaseUpdatePacket(int turretId, int phasePos, int planetId)
    {
        TurretId = turretId;
        PhasePos = phasePos;
        PlanetId = planetId;
    }

    public int TurretId { get; set; }
    public int PhasePos { get; set; }
    public int PlanetId { get; set; }
}
