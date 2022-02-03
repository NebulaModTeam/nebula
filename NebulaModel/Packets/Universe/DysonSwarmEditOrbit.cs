using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSwarmEditOrbitPacket
    {
        public int StarIndex { get; set; }
        public int OrbitId { get; set; }
        public float Radius { get; set; }
        public Float4 Rotation { get; set; }

        public DysonSwarmEditOrbitPacket() { }
        public DysonSwarmEditOrbitPacket(int starIndex, int orbitId, float radius, Quaternion rotation)
        {
            StarIndex = starIndex;
            OrbitId = orbitId;
            Radius = radius;
            Rotation = new Float4(rotation);
        }
    }
}
