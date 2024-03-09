namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGDeactivateUnitPacket
{
    public DFGDeactivateUnitPacket() { }

    public DFGDeactivateUnitPacket(int planetId, int enemyId)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
}
