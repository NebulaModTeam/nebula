namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGFormationAddUnitPacket
{
    public DFGFormationAddUnitPacket() { }

    public DFGFormationAddUnitPacket(int planetId, int baseId, int formId, int portId)
    {
        PlanetId = planetId;
        BaseId = baseId;
        FormId = (byte)formId;
        PortId = (ushort)portId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public byte FormId { get; set; }
    public ushort PortId { get; set; }
}
