#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSwarmAddOrbitPacket
{
    public DysonSwarmAddOrbitPacket() { }

    public DysonSwarmAddOrbitPacket(int starIndex, int orbitId, float radius, Quaternion rotation)
    {
        StarIndex = starIndex;
        OrbitId = orbitId;
        Radius = radius;
        Rotation = new Float4(rotation);
    }

    public int StarIndex { get; set; }
    public int OrbitId { get; set; }
    public float Radius { get; set; }
    public Float4 Rotation { get; set; }
}
