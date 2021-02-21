using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Nebula.UI
{
    internal class UIMainMenuPatcher
    {
        private static RectTransform multiplayerButton;
        private static GameObject multiplayerMenu;

        [HarmonyPatch(typeof(UIMainMenu), "_OnOpen")]
        [HarmonyPostfix]
        public static void PostFix()
        {
            if (GameObject.Find("Overlay Canvas/Main Menu") == null)
                return;

            if (multiplayerButton)
            {
                multiplayerButton.GetComponentInChildren<Text>().text = "Multiplayer";
                return;
            }

            AddMultiplayerButton();
            AddMultiplayerMenu();
        }

        private static void AddMultiplayerButton()
        {
            RectTransform buttonGroup = GameObject.Find("Main Menu/button-group").GetComponent<RectTransform>();
            RectTransform buttonTemplate = GameObject.Find("Main Menu/button-group/button-new").GetComponent<RectTransform>();

            multiplayerButton = GameObject.Instantiate<RectTransform>(buttonTemplate, buttonGroup, false);
            multiplayerButton.name = "button-multiplayer";
            multiplayerButton.anchoredPosition = new Vector2(multiplayerButton.anchoredPosition.x, multiplayerButton.anchoredPosition.y + multiplayerButton.sizeDelta.y + 10);
            multiplayerButton.GetComponentInChildren<Text>().text = "Multiplayer";

            multiplayerButton.GetComponent<Button>().onClick.RemoveAllListeners();
            multiplayerButton.GetComponent<Button>().onClick.AddListener(new UnityAction(OnMultiplayerButtonClick));
        }

        private static void AddMultiplayerMenu()
        {
            multiplayerMenu = new GameObject("Multiplayer Menu");
            multiplayerMenu.AddComponent<MultiplayerMenuUI>();
            multiplayerMenu.SetActive(false);
        }


        private static void OnMultiplayerButtonClick()
        {
            UIRoot.instance.CloseMainMenuUI();
            multiplayerMenu.SetActive(true);
        }
    }
}
