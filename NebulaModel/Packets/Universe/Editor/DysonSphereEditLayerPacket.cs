#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereEditLayerPacket
{
    public DysonSphereEditLayerPacket() { }

    public DysonSphereEditLayerPacket(int starIndex, int layerId, Quaternion orbitRotation)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        OrbitRotation = new Float4(orbitRotation);
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public Float4 OrbitRotation { get; set; }
}
