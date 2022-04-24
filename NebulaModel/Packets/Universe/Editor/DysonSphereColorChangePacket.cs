using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSphereColorChangePacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public Float4 Color { get; set; }
        public ComponentType Type { get; set; }
        public int Index { get; set; }

        public DysonSphereColorChangePacket() { }
        public DysonSphereColorChangePacket(int starIndex, int layerId, Color32 color, ComponentType component, int index)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            Color = Float4.ToFloat4(color);
            Type = component;
            Index = index;
        }

        public enum ComponentType : byte
        {
            Node,
            Frame,
            Shell
        }
    }
}
