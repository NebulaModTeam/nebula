namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DeferredCreateEnemyPacket
{
    public DeferredCreateEnemyPacket() { }

    public DeferredCreateEnemyPacket(int planetId, int baseId, int builderIndex, int enemyId)
    {
        PlanetId = planetId;
        BaseId = baseId;
        BuilderIndex = builderIndex;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public int BuilderIndex { get; set; }
    public int EnemyId { get; set; }
}
