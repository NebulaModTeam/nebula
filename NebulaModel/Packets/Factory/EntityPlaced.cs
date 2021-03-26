using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Factory
{
    public class EntityPlaced
    {
        public int planetId { get; set; }
        public bool isPrebuild { get; set; }
        public short protoId { get; set; }
        public Float3 pos { get; set; }
        public Float4 rot { get; set; }

        public EntityPlaced() { }

        public EntityPlaced(int planetId, short protoId, Vector3 pos, Quaternion rot, bool isPrebuild)
        {
            this.planetId = planetId;
            this.protoId = protoId;
            this.pos = new Float3(pos);
            this.rot = new Float4(rot);
            this.isPrebuild = isPrebuild;
        }
    }
}
