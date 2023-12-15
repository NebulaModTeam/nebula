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

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
    public Float4 Color { get; set; }
    public ComponentType Type { get; set; }
    public int Index { get; set; }
}
