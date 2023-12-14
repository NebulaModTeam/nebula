#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Factory.Inserter;

public class NewSetInserterPickTargetPacket
{
    public NewSetInserterPickTargetPacket() { }

    public NewSetInserterPickTargetPacket(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos, int planetId)
    {
        ObjId = objId;
        OtherObjId = otherObjId;
        InserterId = inserterId;
        Offset = offset;
        PointPos = new Float3(pointPos - pointPos.normalized * 0.15f);
        PlanetId = planetId;
    }

    public int ObjId { get; }
    public int OtherObjId { get; }
    public int InserterId { get; }
    public int Offset { get; }
    public Float3 PointPos { get; }
    public int PlanetId { get; }
}
