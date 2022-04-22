using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIDEToolbox))]
    internal class UIDEToolbox_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIDEToolbox.OnColorChange))]
        public static void OnColorChange_Postfix(UIDEToolbox __instance, Color32 color)
        {
            if (Multiplayer.IsActive && __instance.editor.selection.singleSelectedLayer != null)
            {
                int starIndex = __instance.editor.selection.viewStar.index;
                int layerId = __instance.editor.selection.singleSelectedLayer.id;
                color.a = byte.MaxValue;
                foreach (DysonNode node in __instance.editor.selection.selectedNodes)
                {
                    Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color, DysonSphereColorChangePacket.ComponentType.Node, node.id));
                }
                foreach (DysonFrame frame in __instance.editor.selection.selectedFrames)
                {
                    Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color, DysonSphereColorChangePacket.ComponentType.Frame, frame.id));
                }
                foreach (DysonShell shell in __instance.editor.selection.selectedShells)
                {
                    Multiplayer.Session.Network.SendPacket(new DysonSphereColorChangePacket(starIndex, layerId, color, DysonSphereColorChangePacket.ComponentType.Shell, shell.id));
                }
            }
        }
    }
}
