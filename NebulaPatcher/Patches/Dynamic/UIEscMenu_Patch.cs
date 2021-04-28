using HarmonyLib;
using NebulaWorld;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEscMenu))]
    class UIEscMenu_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Prefix(UIEscMenu __instance)
        {
            // Disable save game button if you are a client in a multiplayer session
            Button saveGameWindowButton = AccessTools.Field(typeof(UIEscMenu), "button2").GetValue(__instance) as Button;
            SetButtonEnableState(saveGameWindowButton, !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient);

            // Disable load game button if in a multiplayer session
            Button loadGameWindowButton = AccessTools.Field(typeof(UIEscMenu), "button3").GetValue(__instance) as Button;
            SetButtonEnableState(loadGameWindowButton, !SimulatedWorld.Initialized);
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnButton5Click")]
        [HarmonyPatch("OnButton6Click")]
        public static void OnGameQuit_Prefix()
        {
            if (SimulatedWorld.Initialized)
            {
                LocalPlayer.LeaveGame();
            }
        }

        private static void SetButtonEnableState(Button button, bool enable)
        {
            ColorBlock buttonColors = button.colors;
            buttonColors.disabledColor = new Color(1f, 1f, 1f, 0.15f);
            button.interactable = enable;
            button.colors = buttonColors;
        }
    }
}
