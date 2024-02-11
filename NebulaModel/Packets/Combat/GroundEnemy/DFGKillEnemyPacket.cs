namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGKillEnemyPacket
{
    public DFGKillEnemyPacket() { }

    public DFGKillEnemyPacket(int planetId, int enemyId)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
}
