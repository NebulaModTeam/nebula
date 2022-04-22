using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSphereAddLayerPacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public float OrbitRadius { get; set; }
        public Float4 OrbitRotation { get; set; }
        public float OrbitAngularSpeed { get; set; }

        public DysonSphereAddLayerPacket() { }
        public DysonSphereAddLayerPacket(int starIndex, int layerId, float orbitRadius, Quaternion orbitRotation, float orbitAngularSpeed)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            OrbitRadius = orbitRadius;
            OrbitRotation = new Float4(orbitRotation);
            OrbitAngularSpeed = orbitAngularSpeed;
        }
    }
}