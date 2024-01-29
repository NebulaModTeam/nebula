namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DeferredRemoveEnemyPacket
{
    public DeferredRemoveEnemyPacket() { }

    public DeferredRemoveEnemyPacket(int planetId, int enemyId)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
}
