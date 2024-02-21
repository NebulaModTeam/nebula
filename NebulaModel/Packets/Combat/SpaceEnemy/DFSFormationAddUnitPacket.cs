namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSFormationAddUnitPacket
{
    public DFSFormationAddUnitPacket() { }

    public DFSFormationAddUnitPacket(int hiveAstroId, int formId, int portId)
    {
        HiveAstroId = hiveAstroId;
        FormId = (byte)formId;
        PortId = (ushort)portId;
    }

    public int HiveAstroId { get; set; }
    public byte FormId { get; set; }
    public ushort PortId { get; set; }
}
