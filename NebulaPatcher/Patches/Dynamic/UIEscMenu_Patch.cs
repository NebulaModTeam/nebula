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
        [HarmonyPatch(nameof(UIEscMenu._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Prefix(UIEscMenu __instance)
        {
            // Disable save game button if you are a client in a multiplayer session
            Button saveGameWindowButton = __instance.button2;
            SetButtonEnableState(saveGameWindowButton, !Multiplayer.IsActive || LocalPlayer.IsMasterClient);

            // Disable load game button if in a multiplayer session
            Button loadGameWindowButton = __instance.button3;
            SetButtonEnableState(loadGameWindowButton, !Multiplayer.IsActive);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIEscMenu.OnButton5Click))]
        public static void OnButton5Click_Prefix()
        {
            QuitGame();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIEscMenu.OnButton6Click))]
        public static void OnButton6Click_Prefix()
        {
            QuitGame();
        }

        private static void QuitGame()
        {
            if (Multiplayer.IsActive)
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
