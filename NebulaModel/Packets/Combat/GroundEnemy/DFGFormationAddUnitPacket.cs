namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGFormationAddUnitPacket
{
    public DFGFormationAddUnitPacket() { }

    public DFGFormationAddUnitPacket(int planetId, int baseId, int formId)
    {
        PlanetId = planetId;
        BaseId = baseId;
        FormId = formId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public int FormId { get; set; }
}
