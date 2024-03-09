namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSAddIdleRelayPacket
{
    public DFSAddIdleRelayPacket() { }

    public DFSAddIdleRelayPacket(int hiveAstroId, int dockIndex, int enemyId)
    {
        HiveAstroId = hiveAstroId;
        DockIndex = dockIndex;
        EnemyId = enemyId;
    }

    public int HiveAstroId { get; set; }
    public int DockIndex { get; set; }
    public int EnemyId { get; set; }
}
