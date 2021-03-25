using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Factory
{
    public class EntityPlaced
    {
        public short protoId { get; set; }
        public Float3 pos { get; set; }
        public Float4 rot { get; set; }

        public EntityPlaced() { }

        public EntityPlaced(short protoId, Vector3 pos, Quaternion rot)
        {
            this.protoId = protoId;
            this.pos = new Float3(pos);
            this.rot = new Float4(rot);
        }
    }
}
