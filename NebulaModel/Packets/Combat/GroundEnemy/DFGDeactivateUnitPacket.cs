namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGDeactivateUnitPacket
{
    public DFGDeactivateUnitPacket() { }

    public DFGDeactivateUnitPacket(int planetId, int unitId)
    {
        PlanetId = planetId;
        UnitId = unitId;
    }

    public int PlanetId { get; set; }
    public int UnitId { get; set; }
}
