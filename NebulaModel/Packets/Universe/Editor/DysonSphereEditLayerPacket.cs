using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSphereEditLayerPacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public Float4 OrbitRotation { get; set; }

        public DysonSphereEditLayerPacket() { }
        public DysonSphereEditLayerPacket(int starIndex, int layerId, Quaternion orbitRotation)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            OrbitRotation = new Float4(orbitRotation);
        }
    }
}
