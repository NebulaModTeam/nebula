using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDysonPaintingGrid))]
    internal class UIDysonPaintingGrid_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDysonPaintingGrid.PaintCells))]
        public static void PaintCells_Postfix(UIDysonPaintingGrid __instance, Color32 paint)
        {
            DysonSphereLayer layer = __instance.editor.selection.singleSelectedLayer;
            if (Multiplayer.IsActive && __instance.cursorCells != null && layer != null)
            {
                float strength = __instance.editor.brush_paint.strength;
                bool superBrightMode = __instance.editor.brush_paint.superBrightMode;
                Multiplayer.Session.Network.SendPacket(new DysonSpherePaintCellsPacket(layer.starData.index, layer.id, paint, strength, superBrightMode, __instance.cursorCells, __instance.cellCount));
            }
        }
    }
}
