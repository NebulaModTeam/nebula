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
            this.StarIndex = starIndex;
            this.OrbitRadius = orbitRadius;
            this.OrbitRotation = new Float4(orbitRotation);
            this.OrbitAngularSpeed = orbitAngularSpeed;
        }
    }
}