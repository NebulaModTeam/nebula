namespace NebulaModel.Packets.Combat.SpaceEnemy;

public class DFSFormationAddUnitPacket
{
    public DFSFormationAddUnitPacket() { }

    public DFSFormationAddUnitPacket(int hiveAstroId, int formId, int portId)
    {
        HiveAstroId = hiveAstroId;
        FormId = formId;
        PortId = portId;
    }

    public int HiveAstroId { get; set; }
    public int FormId { get; set; }
    public int PortId { get; set; }
}
