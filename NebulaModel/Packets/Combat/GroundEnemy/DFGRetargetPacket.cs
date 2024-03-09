namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGRetargetPacket
{
    public DFGRetargetPacket() { }

    public DFGRetargetPacket(int planetId, int enemyId, int target)
    {
        PlanetId = planetId;
        EnemyId = enemyId;
        Target = target;
    }

    public int PlanetId { get; set; }
    public int EnemyId { get; set; }
    public int Target { get; set; }
}
