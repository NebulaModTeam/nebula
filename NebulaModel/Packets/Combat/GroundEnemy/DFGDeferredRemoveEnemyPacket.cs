namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGDeferredRemoveEnemyPacket
{
    public DFGDeferredRemoveEnemyPacket() { }

    public DFGDeferredRemoveEnemyPacket(int planetId, int enemyId)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
}
