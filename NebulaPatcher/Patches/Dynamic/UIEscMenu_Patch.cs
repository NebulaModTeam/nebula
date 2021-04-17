using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;
using NebulaWorld;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIEscMenu))]
    class UIEscMenu_Patch
    {
        private static RectTransform hostGameButton;

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

            // If we are in a multiplayer game already make sure to hide the host game button
            if (SimulatedWorld.Initialized)
            {
                hostGameButton?.gameObject.SetActive(false);
                return;
            }

            if (hostGameButton != null)
            {
                hostGameButton.gameObject.SetActive(true);
                hostGameButton.GetComponentInChildren<Text>().text = "Host Game";
                return;
            }

            RectTransform buttonTemplate = GameObject.Find("Esc Menu/button (6)").GetComponent<RectTransform>();
            hostGameButton = Object.Instantiate(buttonTemplate, buttonTemplate.parent, false);
            hostGameButton.name = "button-host-game";
            hostGameButton.anchoredPosition = new Vector2(buttonTemplate.anchoredPosition.x, buttonTemplate.anchoredPosition.y - buttonTemplate.sizeDelta.y * 2);
            hostGameButton.GetComponentInChildren<Text>().text = "Host Game";

            hostGameButton.GetComponent<Button>().onClick.RemoveAllListeners();
            hostGameButton.GetComponent<Button>().onClick.AddListener(new UnityAction(OnHostCurrentGameClick));
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

        private static void OnHostCurrentGameClick()
        {
            // Make sure to save the game before enabling the multiplayer mod
            GameSave.AutoSave();

            int port = Config.DefaultPort;

            Log.Info($"Listening server on port {port}");
            var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
            session.StartServer(port, true);

            // Manually call the OnGameLoadCompleted manually since we are already in a game.
            SimulatedWorld.OnGameLoadCompleted();

            GameMain.Resume();
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
