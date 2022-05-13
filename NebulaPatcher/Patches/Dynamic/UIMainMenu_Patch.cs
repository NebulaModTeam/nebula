using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaNetwork;
using NebulaWorld;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMainMenu))]
    internal class UIMainMenu_Patch
    {
        private static RectTransform mainMenuButtonGroup;
        private static RectTransform multiplayerButton;
        private static RectTransform multiplayerSubMenu;

        private static RectTransform multiplayerMenu;
        private static InputField hostIPAddressInput;
        private static InputField passwordInput;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIMainMenu._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix()
        {
            Multiplayer.IsLeavingGame = false;

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

        public static void OnMultiplayerButtonClick()
        {
            Multiplayer.IsInMultiplayerMenu = true;
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
            Log.Info($"Listening server on port {Config.Options.HostPort}");
            Multiplayer.HostGame(new Server(Config.Options.HostPort));

            Multiplayer.Session.IsInLobby = true;

            UIRoot.instance.galaxySelect._Open();
            UIRoot.instance.uiMainMenu._Close();
        }

        private static void OnMultiplayerLoadGameButtonClick()
        {
            UIRoot.instance.OpenLoadGameWindow();
        }

        private static void OnMultiplayerJoinGameButtonClick()
        {
            Multiplayer.ShouldReturnToJoinMenu = true;

            UIRoot.instance.CloseMainMenuUI();
            multiplayerMenu.gameObject.SetActive(true);
            hostIPAddressInput.characterLimit = 53;
        }

        private static void OnMultiplayerBackButtonClick()
        {
            Multiplayer.IsInMultiplayerMenu = false;
            Multiplayer.ShouldReturnToJoinMenu = true;
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
            multiplayerMenu.Find("left-group").gameObject.SetActive(false);
            multiplayerMenu.Find("property-multiplier").gameObject.SetActive(false);
            multiplayerMenu.Find("seed-key").gameObject.SetActive(false);

            Transform topTitle = multiplayerMenu.Find("top-title");
            topTitle.GetComponent<Localizer>().enabled = false;
            topTitle.GetComponent<Text>().text = "Multiplayer";

            Transform hostIpField = multiplayerMenu.Find("galaxy-seed");
            hostIpField.GetComponent<Localizer>().enabled = false;
            hostIpField.GetComponent<Text>().text = "Host IP Address";
            hostIPAddressInput = hostIpField.GetComponentInChildren<InputField>();
            hostIPAddressInput.onEndEdit.RemoveAllListeners();
            hostIPAddressInput.onValueChanged.RemoveAllListeners();
            //note: connectToUrl uses Dns.getHostEntry, which can only use up to 255 chars.
            //256 will trigger an argument out of range exception
            hostIPAddressInput.characterLimit = 255;

            string ip = "127.0.0.1";
            if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
            {
                ip = Config.Options.LastIP;
            }
            hostIPAddressInput.text = ip;

            Transform passwordField = Object.Instantiate(hostIpField, hostIpField.parent, false);
            passwordField.localPosition = multiplayerMenu.Find("star-count").localPosition;
            passwordField.GetComponent<Text>().text = "Password (optional)";
            passwordInput = passwordField.GetComponentInChildren<InputField>();
            passwordInput.contentType = InputField.ContentType.Password;

            passwordInput.text = "";
            if (Config.Options.RememberLastClientPassword && !string.IsNullOrEmpty(Config.Options.LastClientPassword))
            {
                passwordInput.text = Config.Options.LastClientPassword;
            }

            OverrideButton(multiplayerMenu.Find("start-button").GetComponent<RectTransform>(), "Join Game", OnJoinGameButtonClick);
            OverrideButton(multiplayerMenu.Find("cancel-button").GetComponent<RectTransform>(), null, OnJoinGameBackButtonClick);
            multiplayerMenu.Find("random-button").gameObject.SetActive(false);

            multiplayerMenu.gameObject.SetActive(false);
        }

        private static void OnJoinGameButtonClick()
        {
            string s = new string(hostIPAddressInput.text.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
            JoinGame(s, passwordInput.text);
        }

        public static void JoinGame(string ip, string password = "")
        {
            // Remove whitespaces from connection string
            string s = ip;

            // Taken from .net IPEndPoint
            IPEndPoint result = null;
            int addressLength = s.Length;  // If there's no port then send the entire string to the address parser
            int lastColonPos = s.LastIndexOf(':');

            // Look to see if this is an IPv6 address with a port.
            if (lastColonPos > 0)
            {
                if (s[lastColonPos - 1] == ']')
                {
                    addressLength = lastColonPos;
                }
                // Look to see if this is IPv4 with a port (IPv6 will have another colon)
                else if (s.Substring(0, lastColonPos).LastIndexOf(':') == -1)
                {
                    addressLength = lastColonPos;
                }
            }

            if (IPAddress.TryParse(s.Substring(0, addressLength), out IPAddress address))
            {
                uint port = 0;
                if (addressLength == s.Length ||
                    (uint.TryParse(s.Substring(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= IPEndPoint.MaxPort))

                {
                    result = new IPEndPoint(address, (int)port);
                }
            }

            bool isIP = false;
            int p = 0;
            if (result != null)
            {
                if (result.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    s = $"[{result.Address}]";
                }
                else
                {
                    s = $"{result.Address}";
                }
                p = result.Port;
                isIP = true;
            }
            else
            {
                string[] tmpP = s.Split(':');
                if (tmpP.Length == 2)
                {
                    if (!int.TryParse(tmpP[1], out p))
                    {
                        p = 0;
                    }
                    else
                    {
                        s = tmpP[0];
                    }
                }
            }

            p = p == 0 ? Config.Options.HostPort : p;

            UIRoot.instance.StartCoroutine(TryConnectToServer(s, p, isIP, password));
        }

        private static IEnumerator TryConnectToServer(string ip, int port, bool isIP, string password)
        {
            InGamePopup.ShowInfo("Connecting", $"Connecting to server {ip}:{port}...", null, null);
            multiplayerMenu.gameObject.SetActive(false);

            // We need to wait here to have time to display the Connecting popup since the game freezes during the connection.
            yield return new WaitForSeconds(0.5f);

            if (!ConnectToServer(ip, port, isIP, password))
            {
                InGamePopup.FadeOut();
                //re-enabling the menu again after failed connect attempt
                InGamePopup.ShowWarning("Connect failed", $"Was not able to connect to {hostIPAddressInput.text}", "OK");
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

        private static bool ConnectToServer(string connectionString, int serverPort, bool isIP, string password)
        {
            if (isIP)
            {
                Multiplayer.JoinGame(new Client(new IPEndPoint(IPAddress.Parse(connectionString), serverPort), password));
                return true;
            }

            //trying to resolve as uri
            if (System.Uri.TryCreate(connectionString, System.UriKind.RelativeOrAbsolute, out _))
            {
                Multiplayer.JoinGame(new Client(connectionString, serverPort, password));
                return true;
            }

            return false;
        }
    }
}
