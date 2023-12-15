#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereAddLayerPacket
{
    public DysonSphereAddLayerPacket() { }

    public DysonSphereAddLayerPacket(int starIndex, int layerId, float orbitRadius, Quaternion orbitRotation,
        float orbitAngularSpeed)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        OrbitRadius = orbitRadius;
        OrbitRotation = new Float4(orbitRotation);
        OrbitAngularSpeed = orbitAngularSpeed;
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public float OrbitRadius { get; set; }
    public Float4 OrbitRotation { get; set; }
    public float OrbitAngularSpeed { get; set; }
}
