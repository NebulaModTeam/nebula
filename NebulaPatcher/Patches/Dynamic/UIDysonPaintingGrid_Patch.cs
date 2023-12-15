#region

using HarmonyLib;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIDysonPaintingGrid))]
internal class UIDysonPaintingGrid_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDysonPaintingGrid.PaintCells))]
    public static void PaintCells_Postfix(UIDysonPaintingGrid __instance, Color32 paint)
    {
        var layer = __instance.editor.selection.singleSelectedLayer;
        if (!Multiplayer.IsActive || __instance.cursorCells == null || layer == null)
        {
            return;
        }
        var strength = __instance.editor.brush_paint.strength;
        var superBrightMode = __instance.editor.brush_paint.superBrightMode;
        Multiplayer.Session.Network.SendPacket(new DysonSpherePaintCellsPacket(layer.starData.index, layer.id, paint,
            strength, superBrightMode, __instance.cursorCells, __instance.cellCount));
    }
}
