using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Factory.Inserter
{
    public class NewSetInserterInsertTargetPacket
    {
        public int ObjId { get; set; }
        public int OtherObjId { get; set; }
        public int InserterId { get; set; }
        public int Offset { get; set; }
        public Float3 PointPos { get; set; }
        public int FactoryIndex { get; set; }

        public NewSetInserterInsertTargetPacket() {}
        public NewSetInserterInsertTargetPacket(int objId, int otherObjId, int inserterId, int offset, Vector3 pointPos, int factoryIndex)
        {
            ObjId = objId;
            OtherObjId = otherObjId;
            InserterId = inserterId;
            Offset = offset;
            PointPos = new Float3(pointPos - pointPos.normalized * 0.15f);
            FactoryIndex = factoryIndex;
        }
    }
}
