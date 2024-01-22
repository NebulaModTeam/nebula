namespace NebulaModel.Packets.Combat.Mecha;

public class MechaShootPacket
{
    public MechaShootPacket() { }

    public MechaShootPacket(ushort playerId, byte ammoType, int ammoItemId, int targetAstroId, int targetId)
    {
        PlayerId = playerId;
        AmmoType = ammoType;
        AmmoItemId = ammoItemId;
        TargetAstroId = targetAstroId;
        TargetId = targetId;
    }

    public ushort PlayerId { get; set; }
    public byte AmmoType { get; set; }
    public int AmmoItemId { get; set; }
    public int TargetAstroId { get; set; }
    public int TargetId { get; set; }
}
