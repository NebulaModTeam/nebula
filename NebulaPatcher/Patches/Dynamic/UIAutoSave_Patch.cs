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
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Postfix(UIAutoSave __instance)
        {
            // Hide AutoSave failed message on clients, since client cannot save in multiplayer
            CanvasGroup contentCanvas = __instance.contentCanvas;
            contentCanvas?.gameObject.SetActive(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient);
            Log.Warn($"UIAutoSave active: {contentCanvas?.gameObject.activeSelf}");
        }

        [HarmonyPrefix]
        [HarmonyPatch("_OnLateUpdate")]
        public static bool _OnLateUpdate_Prefix()
        {
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }
    }
}
