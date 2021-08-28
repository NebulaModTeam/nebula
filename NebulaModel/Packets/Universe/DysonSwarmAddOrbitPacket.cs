using NebulaAPI;
using NebulaModel.DataStructures;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSwarmAddOrbitPacket
    {
        public int StarIndex { get; set; }
        public float Radius { get; set; }
        public Float4 Rotation { get; set; }

        public DysonSwarmAddOrbitPacket() { }
        public DysonSwarmAddOrbitPacket(int starIndex, float radius, Quaternion rotation)
        {
            this.StarIndex = starIndex;
            this.Radius = radius;
            this.Rotation = new Float4(rotation);
        }
    }
}
