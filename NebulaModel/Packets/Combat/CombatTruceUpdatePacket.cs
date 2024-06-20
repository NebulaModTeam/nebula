namespace NebulaModel.Packets.Combat;

public class CombatTruceUpdatePacket
{
    public CombatTruceUpdatePacket() { }

    public CombatTruceUpdatePacket(ushort playerId, long truceEndTime)
    {
        PlayerId = playerId;
        TruceEndTime = truceEndTime;
    }

    public ushort PlayerId { get; set; }
    public long TruceEndTime { get; set; }
}
