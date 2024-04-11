namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGRemoveBasePacket
{
    public DFGRemoveBasePacket() { }

    public DFGRemoveBasePacket(int planetId, int baseId)
    {
        PlanetId = planetId;
        BaseId = baseId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
}
