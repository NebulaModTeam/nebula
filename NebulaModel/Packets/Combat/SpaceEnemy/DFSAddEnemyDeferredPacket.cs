namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSAddEnemyDeferredPacket
{
    public DFSAddEnemyDeferredPacket() { }

    public DFSAddEnemyDeferredPacket(int hiveAstroId, int builderIndex, int enemyId)
    {
        HiveAstroId = hiveAstroId;
        BuilderIndex = builderIndex;
        EnemyId = enemyId;
    }

    public int HiveAstroId { get; set; }
    public int BuilderIndex { get; set; }
    public int EnemyId { get; set; }
}
