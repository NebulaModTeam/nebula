#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSwarmEditOrbitPacket
{
    public DysonSwarmEditOrbitPacket() { }

    public DysonSwarmEditOrbitPacket(int starIndex, int orbitId, float radius, Quaternion rotation)
    {
        StarIndex = starIndex;
        OrbitId = orbitId;
        Radius = radius;
        Rotation = new Float4(rotation);
    }

    public DysonSwarmEditOrbitPacket(int starIndex, int orbitId, Vector4 color)
    {
        StarIndex = starIndex;
        OrbitId = orbitId;
        Color = new Float4(color.x, color.y, color.z, color.w);
        Radius = -1;
    }

    public int StarIndex { get; set; }
    public int OrbitId { get; set; }
    public float Radius { get; set; }
    public Float4 Rotation { get; set; }
    public Float4 Color { get; set; }
}
