using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaPatcher.MonoBehaviours;
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
    class UIMainMenu_Patch
    {
        private static RectTransform mainMenuButtonGroup;
        private static RectTransform multiplayerButton;
        private static RectTransform multiplayerSubMenu;

        private static RectTransform multiplayerMenu;
        private static InputField hostIPAdressInput;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIMainMenu._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
            hostIPAdressInput.characterLimit = 53;
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

            string ip = "127.0.0.1";
            if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
            {
                ip = Config.Options.LastIP;
            }
            hostIPAdressInput.text = ip;

            OverrideButton(multiplayerMenu.Find("start-button").GetComponent<RectTransform>(), "Join Game", OnJoinGameButtonClick);
            OverrideButton(multiplayerMenu.Find("cancel-button").GetComponent<RectTransform>(), null, OnJoinGameBackButtonClick);
            multiplayerMenu.Find("random-button").gameObject.SetActive(false);

            multiplayerMenu.gameObject.SetActive(false);
        }

        private static void OnJoinGameButtonClick()
        {
            // Remove whitespaces from connection string
            var s = new string(hostIPAdressInput.text.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

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

            var isIP = false;
            var p = 0;
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
                var tmpP = s.Split(':');
                if (tmpP.Length == 2)
                {
                    if (!System.Int32.TryParse(tmpP[1], out p))
                        p = 0;
                    else
                        s = tmpP[0];
                }
            }

            p = p == 0 ? Config.Options.HostPort : p;

            UIRoot.instance.StartCoroutine(TryConnectToServer(s, p, isIP));
        }

        private static IEnumerator TryConnectToServer(string ip, int port, bool isIP)
        {
            InGamePopup.ShowInfo("Connecting", $"Connecting to server {ip}:{port}...", null, null);
            multiplayerMenu.gameObject.SetActive(false);

            // We need to wait here to have time to display the Connecting popup since the game freezes during the connection.
            yield return new WaitForSeconds(0.5f);

            if (!ConnectToServer(ip, port, isIP))
            {
                InGamePopup.FadeOut();
                //re-enabling the menu again after failed connect attempt
                InGamePopup.ShowWarning("Connect failed", $"Was not able to connect to {hostIPAdressInput.text}", "OK");
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

        private static bool ConnectToServer(string connectionString, int serverPort, bool isIP)
        {
            NebulaNetwork.MultiplayerClientSession session = NebulaBootstrapper.Instance.CreateMultiplayerClientSession();

            if (isIP)
            {
                session.ConnectToIp(new IPEndPoint(IPAddress.Parse(connectionString), serverPort));
                return true;
            }

            //trying to resolve as uri
            if (System.Uri.TryCreate(connectionString, System.UriKind.RelativeOrAbsolute, out _))
            {
                session.ConnectToUrl(connectionString, serverPort);
                return true;
            }

            session.DestroySession();
            return false;
        }
    }
}
