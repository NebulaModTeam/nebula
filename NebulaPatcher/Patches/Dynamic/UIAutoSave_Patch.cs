using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIAutoSave))]
    class UIAutoSave_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIAutoSave._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix(UIAutoSave __instance)
        {
            // Hide AutoSave failed message on clients, since client cannot save in multiplayer
            CanvasGroup contentCanvas = __instance.contentCanvas;
            if(contentCanvas == null) return;
            contentCanvas.gameObject.SetActive(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient);
            Log.Warn($"UIAutoSave active: {contentCanvas.gameObject.activeSelf}");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIAutoSave._OnLateUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static bool _OnLateUpdate_Prefix()
        {
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }
    }
}
