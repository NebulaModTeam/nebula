namespace NebulaModel.Packets.Combat.GroundEnemy;

public class DFGRemoveBasePitPacket
{
    public DFGRemoveBasePitPacket() { }

    public DFGRemoveBasePitPacket(int planetId, int baseId)
    {
        PlanetId = planetId;
        BaseId = baseId;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
}
