namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSRemoveEnemyDeferredPacket
{
    public DFSRemoveEnemyDeferredPacket() { }

    public DFSRemoveEnemyDeferredPacket(int enemyId)
    {
        EnemyId = enemyId;
    }

    public int EnemyId { get; set; }
}
