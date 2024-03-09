namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGDeferredCreateEnemyPacket
{
    public DFGDeferredCreateEnemyPacket() { }

    public DFGDeferredCreateEnemyPacket(int planetId, int baseId, int builderIndex, int enemyId)
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
