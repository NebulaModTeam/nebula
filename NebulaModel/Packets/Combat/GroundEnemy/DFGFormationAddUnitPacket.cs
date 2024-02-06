namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGFormationAddUnitPacket
{
    public DFGFormationAddUnitPacket() { }

    public DFGFormationAddUnitPacket(int planetId, int baseId, int formId, int portId)
    {
        PlanetId = planetId;
        BaseId = baseId;
        FormId = formId;
        PortId = portId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public int FormId { get; set; }
    public int PortId { get; set; }
}
