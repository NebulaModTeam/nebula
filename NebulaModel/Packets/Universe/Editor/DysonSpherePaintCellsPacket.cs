﻿using NebulaAPI;
using UnityEngine;

namespace NebulaModel.Packets.Universe
{
    public class DysonSpherePaintCellsPacket
    {
        public int StarIndex { get; set; }
        public int LayerId { get; set; }
        public Float4 Paint { get; set; }
        public float Strength { get; set; }
        public bool SuperBrightMode { get; set; }
        public int[] CursorCells { get; set; }
        public int CellCount { get; set; }

        public DysonSpherePaintCellsPacket() { }
        public DysonSpherePaintCellsPacket(int starIndex, int layerId, Color32 paint, float strength, bool superBrightMode, int[] cursorCells, int cellCount)
        {
            StarIndex = starIndex;
            LayerId = layerId;
            Paint = Float4.ToFloat4(paint);
            Strength = strength;
            SuperBrightMode = superBrightMode;
            CursorCells = cursorCells;
            CellCount = cellCount;
        }
    }
}
