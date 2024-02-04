namespace NebulaModel.Packets.Combat.GroundEnemy;

public class ActivateBasePacket
{
    public ActivateBasePacket() { }

    public ActivateBasePacket(int planetId, int baseId, bool setToSeekForm)
    {
        PlanetId = planetId;
        BaseId = baseId;
        SetToSeekForm = setToSeekForm;
    }

    public int PlanetId { get; set; }
    public int BaseId { get; set; }
    public bool SetToSeekForm { get; set; }
}
