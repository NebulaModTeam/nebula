using NebulaAPI;
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
            StarIndex = starIndex;
            Radius = radius;
            Rotation = new Float4(rotation);
        }
    }
}
