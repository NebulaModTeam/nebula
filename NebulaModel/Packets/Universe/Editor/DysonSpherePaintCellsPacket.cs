#region

using NebulaAPI.DataStructures;
using UnityEngine;

#endregion

namespace NebulaModel.Packets.Universe.Editor;

public class DysonSpherePaintCellsPacket
{
    public DysonSpherePaintCellsPacket() { }

    public DysonSpherePaintCellsPacket(int starIndex, int layerId, Color32 paint, float strength, bool superBrightMode,
        int[] cursorCells, int cellCount)
    {
        StarIndex = starIndex;
        LayerId = layerId;
        Paint = Float4.ToFloat4(paint);
        Strength = strength;
        SuperBrightMode = superBrightMode;
        CursorCells = cursorCells;
        CellCount = cellCount;
    }

    public int StarIndex { get; }
    public int LayerId { get; }
    public Float4 Paint { get; }
    public float Strength { get; }
    public bool SuperBrightMode { get; }
    public int[] CursorCells { get; }
    public int CellCount { get; }
}
