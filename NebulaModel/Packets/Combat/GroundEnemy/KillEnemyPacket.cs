namespace NebulaModel.Packets.Combat.GroundEnemy;

public class KillEnemyPacket
{
    public KillEnemyPacket() { }

    public KillEnemyPacket(int planetId, int enemyId)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
}
