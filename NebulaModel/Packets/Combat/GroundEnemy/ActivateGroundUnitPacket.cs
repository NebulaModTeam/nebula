namespace NebulaModel.Packets.Combat.GroundEnemy;

public class ActivateGroundUnitPacket
{
    public ActivateGroundUnitPacket() { }

    public ActivateGroundUnitPacket(int planetId, int baseId, int formId, int portId, byte behavior, byte stateTick)
    {
        PlanetId = planetId;
        BaseId = baseId;
        FormId = formId;
        PortId = portId;
        Behavior = behavior;
        StateTick = stateTick;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public int FormId { get; set; }
    public int PortId { get; set; }
    public byte Behavior { get; set; }
    public byte StateTick { get; set; }
}
