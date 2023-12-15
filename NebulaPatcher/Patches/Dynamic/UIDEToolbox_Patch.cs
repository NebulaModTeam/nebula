#region

using HarmonyLib;
using NebulaModel.Packets.Universe.Editor;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIDEToolbox))]
internal class UIDEToolbox_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIDEToolbox.OnColorChange))]
    public static void OnColorChange_Postfix(UIDEToolbox __instance, Color32 color)
    {
        if (!Multiplayer.IsActive || __instance.editor.selection.singleSelectedLayer == null)
        {
            return;
        }
        var starIndex = __instance.editor.selection.viewStar.index;
        var layerId = __instance.editor.selection.singleSelectedLayer.id;
        color.a = byte.MaxValue;
        foreach (var node in __instance.editor.selection.selectedNodes)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color,
                DysonSphereColorChangePacket.ComponentType.Node, node.id));
        }
        foreach (var frame in __instance.editor.selection.selectedFrames)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color,
                DysonSphereColorChangePacket.ComponentType.Frame, frame.id));
        }
        foreach (var shell in __instance.editor.selection.selectedShells)
        {
            Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color,
                DysonSphereColorChangePacket.ComponentType.Shell, shell.id));
        }
    }
}
