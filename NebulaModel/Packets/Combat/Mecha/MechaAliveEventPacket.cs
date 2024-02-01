namespace NebulaModel.Packets.Combat.Mecha;

public class MechaAliveEventPacket
{
    public MechaAliveEventPacket() { }

    public MechaAliveEventPacket(ushort playerId, EStatus status)
    {
        PlayerId = playerId;
        Status = status;
    }

    public enum EStatus : byte
    {
        Kill,
        RespawnAtOnce,
        RespawnKeepPosition,
        RespawnAtBirthPoint
    }

    public ushort PlayerId { get; set; }
    public EStatus Status { get; set; }
}
