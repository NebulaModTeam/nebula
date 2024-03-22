namespace NebulaModel.Packets.Combat;

public class CombatStatFullHptPacket
{
    public CombatStatFullHptPacket() { }

    public CombatStatFullHptPacket(int originAstroId, int objectType, int objectId)
    {
        OriginAstroId = originAstroId;
        ObjectType = objectType;
        ObjectId = objectId;
    }

    public int OriginAstroId { get; set; }
    public int ObjectType { get; set; }
    public int ObjectId { get; set; }
}
