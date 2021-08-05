using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;
using NebulaWorld;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
            multiplayerSubMenu.Find("button-galaxy").gameObject.SetActive(false);

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
            hostIPAdressInput.characterLimit = 62;
        }

        private static void OnMultiplayerBackButtonClick()
        {
            MainMenuManager.IsInMultiplayerMenu = false;
            multiplayerSubMenu.gameObject.SetActive(false);
            mainMenuButtonGroup.gameObject.SetActive(true);
        }

        private static void OverrideButton(RectTransform buttonObj, string newText, System.Action newClickCallback)
        {
            if (newText != null)
            {
                // Remove the Localizer since we don't support translation for now and it will always revert the text otherwise
                Object.Destroy(buttonObj.GetComponentInChildren<Localizer>());
                buttonObj.GetComponentInChildren<Text>().text = newText;
            }

            Button button = buttonObj.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(new UnityAction(newClickCallback));
            button.interactable = true;
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
            //note: connectToUrl uses Dns.getHostEntry, which can only use up to 255 chars.
            //256 will trigger an argument out of range exception
            hostIPAdressInput.characterLimit = 255;

            string host = "localhost";
            if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
            {
                host = Config.Options.LastIP;
            }
            string ip;
            if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP) &&
                Uri.TryCreate(Config.Options.LastIP, UriKind.Absolute, out Uri result))
            {
                ip = result.ToString();
            }
            else
            {
                ip = new UriBuilder(NebulaNetwork.MirrorManager.DefaultScheme, host, Config.Options.HostPort).Uri.ToString();
            }
            hostIPAdressInput.text = ip;

            OverrideButton(multiplayerMenu.Find("start-button").GetComponent<RectTransform>(), "Join Game", OnJoinGameButtonClick);
            OverrideButton(multiplayerMenu.Find("cancel-button").GetComponent<RectTransform>(), null, OnJoinGameBackButtonClick);
            multiplayerMenu.Find("random-button").gameObject.SetActive(false);

            multiplayerMenu.gameObject.SetActive(false);
        }

        private static void OnJoinGameButtonClick()
        {
            // Remove erraneous characters from connection string
            var s = new string(hostIPAdressInput.text.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray()).Trim(new char[] { '\'', '"' });

            Regex ipv6 = new Regex(@"^((?:\w+:\/\/?)?\[?(?:(?:\w{0,4}?:){7}\w{0,4}?|0{0,4}:0{0,4}:FFFF:(?:\d{1,3}\.){3}\d{1,3})\]?)(?::(\d+))?$", RegexOptions.IgnoreCase);
            Regex everythingElse = new Regex(@"^(.+?)(?::(\d+))?$");

            var ipv6Matches = ipv6.Matches(s);
            if (ipv6Matches.Count == 2)
            {
                if (int.TryParse(ipv6Matches[1].ToString(), out int port))
                {
                    UriBuilder uri = new UriBuilder(s.Take(s.Length - port.ToString().Length - 1).ToArray().ToString())
                    {
                        Port = port
                    };
                    UIRoot.instance.StartCoroutine(TryConnectToServer(uri.Uri));
                    return;
                }
            }

            var everythingElseMatches = everythingElse.Matches(s);
            if (everythingElseMatches.Count == 2)
            {
                if (int.TryParse(everythingElseMatches[1].ToString(), out int port))
                {
                    UriBuilder uri = new UriBuilder(s.Take(s.Length - port.ToString().Length - 1).ToArray().ToString())
                    {
                        Port = port
                    };
                    UIRoot.instance.StartCoroutine(TryConnectToServer(uri.Uri));
                    return;
                }
            }

            if (Uri.TryCreate(s, UriKind.Absolute, out Uri result) || 
                Uri.TryCreate($"{NebulaNetwork.MirrorManager.DefaultScheme}://{s}", UriKind.Absolute, out result))
            {
                if(result.IsDefaultPort)
                {
                    result = new UriBuilder(result.Scheme, result.Host, Config.Options.HostPort).Uri;
                }
                UIRoot.instance.StartCoroutine(TryConnectToServer(result));
            }
            else
            {
                InGamePopup.ShowWarning("Connect failed", $"{s} is not a valid host", "OK");
            }
        }

        private static IEnumerator TryConnectToServer(Uri uri)
        {
            InGamePopup.ShowInfo("Connecting", $"Connecting to server {uri}...", null, null);
            multiplayerMenu.gameObject.SetActive(false);

            // We need to wait here to have time to display the Connecting popup since the game freezes during the connection.
            yield return new WaitForSeconds(0.5f);

            if (!ConnectToServer(uri))
            {
                InGamePopup.FadeOut();
                //re-enabling the menu again after failed connect attempt
                InGamePopup.ShowWarning("Connect failed", $"Was not able to connect to {uri.OriginalString}", "OK");
                multiplayerMenu.gameObject.SetActive(true);
            }
            else
            {
                InGamePopup.FadeOut();
            }
        }

        private static void OnJoinGameBackButtonClick()
        {
            multiplayerMenu.gameObject.SetActive(false);
            UIRoot.instance.OpenMainMenuUI();
        }

        private static bool ConnectToServer(Uri uri)
        {
            NebulaNetwork.MultiplayerClientSession session = NebulaBootstrapper.Instance.CreateMultiplayerClientSession();

            session.Connect(uri);
            return true;
            /*
            if (isIP)
            {
                session.ConnectToIp(new IPEndPoint(IPAddress.Parse(connectionString), serverPort));
                return true;
            }

            //trying to resolve as uri
            if (System.Uri.TryCreate(connectionString, System.UriKind.RelativeOrAbsolute, out var serverUri))
            {
                session.ConnectToUrl(connectionString, serverPort);
                return true;
            }

            session.DestroySession();
            return false;
            */
        }
    }
}
