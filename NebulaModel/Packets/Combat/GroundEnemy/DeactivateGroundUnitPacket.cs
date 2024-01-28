namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DeactivateGroundUnitPacket
{
    public DeactivateGroundUnitPacket() { }

    public DeactivateGroundUnitPacket(int planetId, int unitId)
    {
        PlanetId = planetId;
        UnitId = unitId;
    }

    public int PlanetId { get; set; }
    public int UnitId { get; set; }
}
