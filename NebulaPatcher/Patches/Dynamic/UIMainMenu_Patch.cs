#region

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaNetwork;
using NebulaWorld;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

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
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix()
    {
        Multiplayer.IsLeavingGame = false;

        var overlayCanvas = GameObject.Find("Overlay Canvas");
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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIMainMenu.OnUpdateLogButtonClick))]
    public static void OnUpdateLogButtonClick_Postfix()
    {
        // Return to main menu when update log is opened
        OnMultiplayerBackButtonClick();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIMainMenu.UpdateDemoScene))]
    [HarmonyPatch(nameof(UIMainMenu._OnUpdate))]
    public static void OnEscSwitch()
    {
        if (!VFInput.escape) return;

        // Go back to the upper level when hitting esc
        if (multiplayerMenu.gameObject.activeInHierarchy)
        {
            OnJoinGameBackButtonClick();
            VFInput.UseEscape();
        }
        else if (UIRoot.instance.loadGameWindow.active)
        {
            UIRoot.instance.loadGameWindow.OnCancelClick(0);
            VFInput.UseEscape();
        }
        else if (multiplayerSubMenu.gameObject.activeInHierarchy)
        {
            OnMultiplayerBackButtonClick();
            VFInput.UseEscape();
        }
    }

    // Main Menu
    private static void AddMultiplayerButton()
    {
        var buttonTemplate = GameObject.Find("Main Menu/button-group/button-new").GetComponent<RectTransform>();
        multiplayerButton = Object.Instantiate(buttonTemplate, mainMenuButtonGroup, false);
        multiplayerButton.name = "button-multiplayer";
        var anchoredPosition = multiplayerButton.anchoredPosition;
        multiplayerButton.anchoredPosition = new Vector2(anchoredPosition.x,
            anchoredPosition.y + multiplayerButton.sizeDelta.y + 10);
        OverrideButton(multiplayerButton, "Multiplayer".Translate(), OnMultiplayerButtonClick);
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

        var newGameButton = OverrideButton(multiplayerSubMenu.Find("button-multiplayer").GetComponent<RectTransform>(),
            "New Game (Host)".Translate(), OnMultiplayerNewGameButtonClick);
        OverrideButton(multiplayerSubMenu.Find("button-new").GetComponent<RectTransform>(), "Load Game (Host)".Translate(),
            OnMultiplayerLoadGameButtonClick);
        OverrideButton(multiplayerSubMenu.Find("button-continue").GetComponent<RectTransform>(), "Join Game".Translate(),
            OnMultiplayerJoinGameButtonClick);
        OverrideButton(multiplayerSubMenu.Find("button-load").GetComponent<RectTransform>(), "Back".Translate(),
            OnMultiplayerBackButtonClick);

        if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("dsp.galactic-scale.2") && newGameButton != null)
        {
            // Because GalacticScale can't enter new game in MP, temporarily hide New Game (Host) button 
            newGameButton.gameObject.SetActive(false);
            Log.Warn("Hide New Game (Host) button due to GalacticScale compatibility");
        }

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

    private static Button OverrideButton(Component buttonObj, string newText, Action newClickCallback)
    {
        if (newText != null)
        {
            // Remove the Localizer since we don't support translation for now and it will always revert the text otherwise
            Object.Destroy(buttonObj.GetComponentInChildren<Localizer>());
            buttonObj.GetComponentInChildren<Text>().text = newText;
        }

        var button = buttonObj.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(new UnityAction(newClickCallback));
        button.interactable = true;
        return button;
    }

    // Multiplayer Join Menu
    private static void AddMultiplayerJoinMenu()
    {
        var overlayCanvasGo = GameObject.Find("Overlay Canvas");
        var galaxySelectGo = overlayCanvasGo.transform.Find("Galaxy Select");
        if (galaxySelectGo == null)
        {
            Log.Warn("'Overlay Canvas/Galaxy Select' not found!");
            return;
        }

        var galaxySelectTemplate = galaxySelectGo.GetComponent<RectTransform>();

        multiplayerMenu = Object.Instantiate(galaxySelectTemplate, galaxySelectTemplate.parent);
        Object.Destroy(multiplayerMenu.gameObject.GetComponent<UIGalaxySelect>());

        multiplayerMenu.gameObject.name = "Nebula - Multiplayer Menu";
        for (var i = 0; i < multiplayerMenu.childCount; i++)
        {
            var child = multiplayerMenu.GetChild(i);
            switch (child.name)
            {
                case "setting-group":
                    for (var j = 0; j < child.childCount; j++)
                    {
                        var child2 = child.GetChild(j);
                        switch (child2.name)
                        {
                            case "top-title":
                                child2.GetComponent<Localizer>().enabled = false;
                                child2.GetComponent<Text>().text = "Multiplayer".Translate();
                                break;
                            case "galaxy-seed":
                                {
                                    child2.GetComponent<Localizer>().enabled = false;
                                    child2.GetComponent<Text>().text = "Host IP Address".Translate();
                                    child2.name = "Host IP Address";
                                    hostIPAddressInput = child2.GetComponentInChildren<InputField>();
                                    hostIPAddressInput.onEndEdit.RemoveAllListeners();
                                    hostIPAddressInput.onValueChanged.RemoveAllListeners();
                                    //note: connectToUrl uses Dns.getHostEntry, which can only use up to 255 chars.
                                    //256 will trigger an argument out of range exception
                                    hostIPAddressInput.characterLimit = 255;

                                    var ip = "127.0.0.1";
                                    if (Config.Options.RememberLastIP && !string.IsNullOrWhiteSpace(Config.Options.LastIP))
                                    {
                                        ip = Config.Options.LastIP;
                                    }
                                    hostIPAddressInput.text = ip;
                                    hostIPAddressInput.contentType = Config.Options.StreamerMode
                                        ? InputField.ContentType.Password
                                        : InputField.ContentType.Standard;
                                    break;
                                }
                            default:
                                // Remove all unused elements that may be added by other mods
                                Object.Destroy(child2.gameObject);
                                break;
                        }
                    }
                    break;
                case "start-button":
                    OverrideButton(multiplayerMenu.Find("start-button").GetComponent<RectTransform>(), "Join Game".Translate(),
                        OnJoinGameButtonClick);
                    break;
                case "cancel-button":
                    OverrideButton(multiplayerMenu.Find("cancel-button").GetComponent<RectTransform>(), null,
                        OnJoinGameBackButtonClick);
                    break;
                default:
                    // Remove all unused elements that may be added by other mods
                    Object.Destroy(child.gameObject);
                    break;
            }
        }
        if (hostIPAddressInput == null)
        {
            Log.Warn("setting-group/galaxy-seed not found!");
        }
        var addressTransform = hostIPAddressInput.transform.parent;
        addressTransform.SetParent(multiplayerMenu);
        addressTransform.localPosition = new Vector3(0, 335, 0);
        var passwordTransform = Object.Instantiate(addressTransform, multiplayerMenu);
        passwordTransform.localPosition += new Vector3(0, -36, 0);
        passwordTransform.GetComponent<Text>().text = "Password (optional)".Translate();
        passwordTransform.name = "Password (optional)";

        passwordInput = passwordTransform.GetComponentInChildren<InputField>();
        passwordInput.contentType = InputField.ContentType.Password;
        passwordInput.text = "";
        if (Config.Options.RememberLastClientPassword && !string.IsNullOrWhiteSpace(Config.Options.LastClientPassword))
        {
            passwordInput.text = Config.Options.LastClientPassword;
        }

        multiplayerMenu.gameObject.SetActive(false);
    }

    private static void OnJoinGameButtonClick()
    {
        var s = new string(hostIPAddressInput.text.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
        JoinGame(s, passwordInput.text);
    }

    public static void JoinGame(string ip, string password = "")
    {
        // Remove whitespaces from connection string
        var s = ip;

        // Parse protocol if set
        var protocol = "ws";
        var firstColonPos = s.IndexOf("://");
        if (firstColonPos > 0)
        {
            var candidate = s.Substring(0, firstColonPos);
            switch (candidate)
            {
                case "wss":
                case "ws":
                    protocol = candidate;
                    s = s.Substring(firstColonPos + 3);
                    break;
            }
        }

        // Taken from .net IPEndPoint
        IPEndPoint result = null;
        var addressLength = s.Length; // If there's no port then send the entire string to the address parser
        var lastColonPos = s.LastIndexOf(':');

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

        if (IPAddress.TryParse(s.Substring(0, addressLength), out var address))
        {
            uint port = 0;
            if (addressLength == s.Length ||
                uint.TryParse(s.Substring(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) &&
                port <= IPEndPoint.MaxPort)

            {
                result = new IPEndPoint(address, (int)port);
            }
        }

        var isIP = false;
        var p = 0;
        if (result != null)
        {
            s = result.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{result.Address}]" : $"{result.Address}";
            p = result.Port;
            isIP = true;
        }
        else
        {
            var tmpP = s.Split(':');
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

        UIRoot.instance.StartCoroutine(TryConnectToServer(s, protocol, p, isIP, password));
    }

    private static IEnumerator TryConnectToServer(string ip, string protocol, int port, bool isIP, string password)
    {
        InGamePopup.ShowInfo("Connecting".Translate(), "Connecting to server...".Translate(), null);
        multiplayerMenu.gameObject.SetActive(false);

        // We need to wait here to have time to display the Connecting popup since the game freezes during the connection.
        yield return new WaitForSeconds(0.5f);

        if (!ConnectToServer(ip, protocol, port, isIP, password))
        {
            InGamePopup.FadeOut();
            //re-enabling the menu again after failed connect attempt
            InGamePopup.ShowWarning("Connect failed".Translate(), "Was not able to connect to server".Translate(), "OK");
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

    private static bool ConnectToServer(string connectionString, string protocol, int serverPort, bool isIP, string password)
    {
        try
        {
            if (isIP)
            {
                Multiplayer.JoinGame(new Client(new IPEndPoint(IPAddress.Parse(connectionString), serverPort), protocol, password));
                return true;
            }

            //trying to resolve as uri
            if (!Uri.TryCreate(connectionString, UriKind.RelativeOrAbsolute, out _))
            {
                return false;
            }
            Multiplayer.JoinGame(new Client(connectionString, serverPort, protocol, password));
            return true;
        }
        catch (Exception e)
        {
            Log.Error("ConnectToServer error:\n" + e);
        }
        return false;
    }
}
