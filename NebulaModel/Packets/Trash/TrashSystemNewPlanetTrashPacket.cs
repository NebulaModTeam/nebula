using NebulaAPI.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Trash;

public class TrashSystemNewPlanetTrashPacket
{
    public TrashSystemNewPlanetTrashPacket() { }

    public TrashSystemNewPlanetTrashPacket(int trashId, in TrashObject trashObject, in TrashData trashData)
    {
        TrashId = trashId;
        PlanetId = trashData.nearPlanetId;
        Pos = trashData.lPos.ToFloat3();
        Life = trashData.life;
        ItemId = trashObject.item;
        Count = trashObject.count;
        Inc = trashObject.inc;
    }

    public int TrashId { get; set; }
    public int PlanetId { get; set; }
    public Float3 Pos { get; set; }
    public int Life { get; set; }
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int Inc { get; set; }
}
