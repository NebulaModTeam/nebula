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

    public int StarIndex { get; }
    public int LayerId { get; }
    public float OrbitRadius { get; }
    public Float4 OrbitRotation { get; }
    public float OrbitAngularSpeed { get; }
}
