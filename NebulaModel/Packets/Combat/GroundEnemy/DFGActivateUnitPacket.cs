namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGActivateUnitPacket
{
    public DFGActivateUnitPacket() { }

    public DFGActivateUnitPacket(int planetId, int baseId, int formId, int portId, byte behavior, int stateTick, int unitId)
    {
        PlanetId = planetId;
        BaseId = (ushort)baseId;
        FormId = (byte)formId;          // current max: 2
        PortId = (ushort)portId;        // current max: 1440
        Behavior = behavior;
        StateTick = (short)stateTick;   // current max: 120
        UnitId = (ushort)unitId;
    }

    public int PlanetId { get; set; }
    public ushort BaseId { get; set; }
    public byte FormId { get; set; }
    public ushort PortId { get; set; }
    public byte Behavior { get; set; }
    public short StateTick { get; set; }
    public ushort UnitId { get; set; }
}
