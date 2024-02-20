namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSDeactivateUnitPacket
{
    public DFSDeactivateUnitPacket() { }

    public DFSDeactivateUnitPacket(int hiveAstorId, int enemyId)
    {
        HiveAstroId = hiveAstorId;
        EnemyId = enemyId;
    }

    public int HiveAstroId { get; set; }
    public int EnemyId { get; set; }
}
