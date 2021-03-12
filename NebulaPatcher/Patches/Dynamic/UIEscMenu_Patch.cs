using HarmonyLib;
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
        public static void _OnOpen()
        {
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
        public static void OnGameQuit()
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

            // TODO: This port should come from the server.config file.
            int port = 8469;

            Log.Info($"Listening server on port {port}");
            var session = NebulaBootstrapper.Instance.CreateMultiplayerHostSession();
            session.StartServer(port);

            // Manually call the OnGameLoadCompleted here since we are already in a game.
            SimulatedWorld.OnGameLoadCompleted();

            GameMain.Resume();
        }
    }
}
