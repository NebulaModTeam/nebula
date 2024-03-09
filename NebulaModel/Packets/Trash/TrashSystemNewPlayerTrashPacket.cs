using NebulaAPI.DataStructures;

namespace NebulaModel.Packets.Trash;

public class TrashSystemNewPlayerTrashPacket
{
    public TrashSystemNewPlayerTrashPacket() { }

    public TrashSystemNewPlayerTrashPacket(ushort playerId, int trashId, in TrashObject trashObject, in TrashData trashData)
    {
        PlayerId = playerId;
        TrashId = trashId;
        NearStarId = trashData.nearStarId;
        Life = trashData.life;
        UVel = new Double3(trashData.uVel.x, trashData.uVel.y, trashData.uVel.z);
        ItemId = trashObject.item;
        Count = trashObject.count;
        Inc = trashObject.inc;
    }

    public ushort PlayerId { get; set; }
    public int TrashId { get; set; }
    public int NearStarId { get; set; }
    public int Life { get; set; }
    public Double3 UVel { get; set; }
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int Inc { get; set; }
}
