namespace NebulaModel.Packets.Combat;

public class CombatAggressivenessUpdatePacket
{
    public CombatAggressivenessUpdatePacket() { }

    public CombatAggressivenessUpdatePacket(ushort playerId, float aggressiveness)
    {
        PlayerId = playerId;
        Aggressiveness = aggressiveness;
    }

    public ushort PlayerId { get; set; }
    public float Aggressiveness { get; set; }
}
