namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSActivateUnitPacket
{
    public DFSActivateUnitPacket() { }

    public DFSActivateUnitPacket(int hiveAstroId, int formId, int portId, byte behavior, int stateTick, int unitId, int enemyId)
    {
        HiveAstroId = hiveAstroId;
        FormId = (byte)formId;          // current max: 2
        PortId = (ushort)portId;        // current max: 1440
        Behavior = behavior;
        StateTick = (short)stateTick;   // current max: 120
        UnitId = (ushort)unitId;
        EnemyId = enemyId;
    }

    public int HiveAstroId { get; set; }
    public byte FormId { get; set; }
    public ushort PortId { get; set; }
    public byte Behavior { get; set; }
    public short StateTick { get; set; }
    public ushort UnitId { get; set; }
    public int EnemyId { get; set; }
}
