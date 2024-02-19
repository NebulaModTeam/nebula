namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSKillEnemyPacket
{
    public DFSKillEnemyPacket() { }

    public DFSKillEnemyPacket(int originAstroId, int enemyId)
    {
        OriginAstroId = originAstroId;
        EnemyId = enemyId;
    }

    public int OriginAstroId { get; set; }
    public int EnemyId { get; set; }
}
