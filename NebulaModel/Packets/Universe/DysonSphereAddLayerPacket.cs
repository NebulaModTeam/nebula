using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSphereAddLayerPacket
    {
        public int StarIndex { get; set; }
        public float OrbitRadius { get; set; }
        public Float4 OrbitRotation { get; set; }
        public float OrbitAngularSpeed { get; set; }

        public DysonSphereAddLayerPacket() { }
        public DysonSphereAddLayerPacket(int starIndex, float orbitRadius, Quaternion orbitRotation, float orbitAngularSpeed)
        {
            StarIndex = starIndex;
            OrbitRadius = orbitRadius;
            OrbitRotation = new Float4(orbitRotation);
            OrbitAngularSpeed = orbitAngularSpeed;
        }
    }
}