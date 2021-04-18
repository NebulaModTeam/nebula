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
    [HarmonyPatch(typeof(UIMainMenu))]
    class UIMainMenu_Patch
    {
        private static RectTransform mainMenuButtonGroup;
        private static RectTransform multiplayerButton;
        private static RectTransform multiplayerSubMenu;

        private static RectTransform multiplayerMenu;
        private static InputField hostIPAdressInput;

        [HarmonyPostfix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Postfix()
        {
            SimulatedWorld.ExitingMultiplayerSession = false;

            GameObject overlayCanvas = GameObject.Find("Overlay Canvas");
            if (overlayCanvas == null)
            {
                Log.Warn("'Overlay Canvas' not found!");
                return;
            }

            if (overlayCanvas.transform.Find("Main Menu") == null)
            {
                Log.Warn("'Overlay Canvas/Main Menu' not found!");
                return;
            }

            // Check if the main menu already includes our modification
            if (mainMenuButtonGroup != null)
            {
                return;
            }

            mainMenuButtonGroup = GameObject.Find("Main Menu/button-group").GetComponent<RectTransform>();

            AddMultiplayerButton();
            AddMultiplayerSubMenu();
            AddMultiplayerJoinMenu();
        }

        // Main Menu
        private static void AddMultiplayerButton()
        {
            RectTransform buttonTemplate = GameObject.Find("Main Menu/button-group/button-new").GetComponent<RectTransform>();
            multiplayerButton = Object.Instantiate(buttonTemplate, mainMenuButtonGroup, false);
            multiplayerButton.name = "button-multiplayer";
            multiplayerButton.anchoredPosition = new Vector2(multiplayerButton.anchoredPosition.x, multiplayerButton.anchoredPosition.y + multiplayerButton.sizeDelta.y + 10);
            OverrideButton(multiplayerButton, "Multiplayer", OnMultiplayerButtonClick);
        }

        private static void OnMultiplayerButtonClick()
        {
            MainMenuManager.IsInMultiplayerMenu = true;
            mainMenuButtonGroup.gameObject.SetActive(false);
            multiplayerSubMenu.gameObject.SetActive(true);
        }

        // Multiplayer Sub Menu
        private static void AddMultiplayerSubMenu()
        {
            multiplayerSubMenu = Object.Instantiate(mainMenuButtonGroup, mainMenuButtonGroup.parent, true);
            multiplayerSubMenu.name = "multiplayer-menu";

            OverrideButton(multiplayerSubMenu.Find("button-multiplayer").GetComponent<RectTransform>(), "New Game (Host)", OnMultiplayerNewGameButtonClick);
            OverrideButton(multiplayerSubMenu.Find("button-new").GetComponent<RectTransform>(), "Load Game (Host)", OnMultiplayerLoadGameButtonClick);
            OverrideButton(multiplayerSubMenu.Find("button-continue").GetComponent<RectTransform>(), "Join Game", OnMultiplayerJoinGameButtonClick);
            OverrideButton(multiplayerSubMenu.Find("button-load").GetComponent<RectTransform>(), "Back", OnMultiplayerBackButtonClick);

            multiplayerSubMenu.Find("button-options").gameObject.SetActive(false);
            multiplayerSubMenu.Find("button-credits").gameObject.SetActive(false);
            multiplayerSubMenu.Find("button-exit").gameObject.SetActive(false);

            multiplayerSubMenu.gameObject.SetActive(false);
        }

        private static void OnMultiplayerNewGameButtonClick()
        {
            UIRoot.instance.galaxySelect._Open();
            UIRoot.instance.uiMainMenu._Close();
        }

        private static void OnMultiplayerLoadGameButtonClick()
        {
            UIRoot.instance.OpenLoadGameWindow();
        }

        private static void OnMultiplayerJoinGameButtonClick()
        {
            UIRoot.instance.CloseMainMenuUI();
            multiplayerMenu.gameObject.SetActive(true);
            hostIPAdressInput.characterLimit = 30;
        }

        private static void OnMultiplayerBackButtonClick()
        {
            MainMenuManager.IsInMultiplayerMenu = false;
            multiplayerSubMenu.gameObject.SetActive(false);
            mainMenuButtonGroup.gameObject.SetActive(true);
        }

        private static void OverrideButton(RectTransform button, string newText, System.Action newClickCallback)
        {
            if (newText != null)
            {
                // Remove the Localizer since we don't support translation for now and it will always revert the text otherwise
                Object.Destroy(button.GetComponentInChildren<Localizer>());
                button.GetComponentInChildren<Text>().text = newText;
            }

            button.GetComponent<Button>().onClick.RemoveAllListeners();
            button.GetComponent<Button>().onClick.AddListener(new UnityAction(newClickCallback));
        }

        // Multiplayer Join Menu
        private static void AddMultiplayerJoinMenu()
        {
            GameObject overlayCanvasGo = GameObject.Find("Overlay Canvas");
            Transform galaxySelectGo = overlayCanvasGo.transform.Find("Galaxy Select");
            if (galaxySelectGo == null)
            {
                Log.Warn("'Overlay Canvas/Galaxy Select' not found!");
                return;
            }

            RectTransform galaxySelectTemplate = galaxySelectGo.GetComponent<RectTransform>();

            multiplayerMenu = Object.Instantiate(galaxySelectTemplate, galaxySelectTemplate.parent);
            Object.Destroy(multiplayerMenu.gameObject.GetComponent<UIGalaxySelect>());

            multiplayerMenu.gameObject.name = "Nebula - Multiplayer Menu";
            multiplayerMenu.Find("star-count").gameObject.SetActive(false);
            multiplayerMenu.Find("resource-multiplier").gameObject.SetActive(false);
            multiplayerMenu.Find("right-group").gameObject.SetActive(false);

            var topTitle = multiplayerMenu.Find("top-title");
            topTitle.GetComponent<Localizer>().enabled = false;
            topTitle.GetComponent<Text>().text = "Multiplayer";

            var hostIpField = multiplayerMenu.Find("galaxy-seed");
            hostIpField.GetComponent<Localizer>().enabled = false;
            hostIpField.GetComponent<Text>().text = "Host IP Address";
            hostIPAdressInput = hostIpField.GetComponentInChildren<InputField>();
            hostIPAdressInput.onEndEdit.RemoveAllListeners();
            hostIPAdressInput.onValueChanged.RemoveAllListeners();
            hostIPAdressInput.characterLimit = 30;
            hostIPAdressInput.text = "127.0.0.1";

            OverrideButton(multiplayerMenu.Find("start-button").GetComponent<RectTransform>(), "Join Game", OnJoinGameButtonClick);
            OverrideButton(multiplayerMenu.Find("cancel-button").GetComponent<RectTransform>(), null, OnJoinGameBackButtonClick);
            multiplayerMenu.Find("random-button").gameObject.SetActive(false);

            multiplayerMenu.gameObject.SetActive(false);
        }

        private static void OnJoinGameButtonClick()
        {
            string[] parts = hostIPAdressInput.text.Split(':');
            string ip = parts[0];
            int port;

            if (parts.Length == 1)
            {
                port = Config.DefaultPort;
            }
            else if (!int.TryParse(parts[1], out port))
            {
                Log.Info($"Port must be a valid number above 1024");
                return;
            }

            // TODO: Should display a loader during the connection and only open the game once the player is connected to the server.
            multiplayerMenu.gameObject.SetActive(false);

            Log.Info($"Connecting to server... {ip}:{port}");

            var session = NebulaBootstrapper.Instance.CreateMultiplayerClientSession();
            session.Connect(ip, port);
        }

        private static void OnJoinGameBackButtonClick()
        {
            multiplayerMenu.gameObject.SetActive(false);
            UIRoot.instance.OpenMainMenuUI();
        }
    }
}
