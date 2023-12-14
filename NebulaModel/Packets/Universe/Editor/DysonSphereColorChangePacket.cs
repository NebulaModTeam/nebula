#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSphereColorChangePacket
{
    public enum ComponentType : byte
    {
        Node,
        Frame,
        Shell
    }

    public DysonSphereColorChangePacket() { }

    public DysonSphereColorChangePacket(int starIndex, int layerId, Color32 color, ComponentType component, int index)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        Color = Float4.ToFloat4(color);
        Type = component;
        Index = index;
    }

    public int StarIndex { get; }
    public int LayerId { get; }
    public Float4 Color { get; }
    public ComponentType Type { get; }
    public int Index { get; }
}
