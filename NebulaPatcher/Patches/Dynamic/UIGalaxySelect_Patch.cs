#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Session;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIGalaxySelect))]
internal class UIGalaxySelect_Patch
{
    private static int MainMenuStarID = -1;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGalaxySelect._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIGalaxySelect __instance)
    {
        if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
        {
            var galaxySelectRect = __instance.gameObject.GetComponent<RectTransform>();
            galaxySelectRect.Find("random-button").gameObject.SetActive(false);
            var settingGroupRect = galaxySelectRect.Find("setting-group");
            for (var i = 0; i < settingGroupRect.childCount; i++)
            {
                var childObject = settingGroupRect.GetChild(i).gameObject;
                if (childObject.name != "top-title" && childObject.name != "galaxy-seed")
                    childObject.SetActive(false);
            }
        }

        if (!Multiplayer.IsActive)
        {
            return;
        }

        // show lobby hints if needed
        if (Config.Options.ShowLobbyHints)
        {
            string message;
            if ("Nebula_LobbyMessage".Translate() != "Nebula_LobbyMessage") // Translation exists
            {
                message = "Nebula_LobbyMessage".Translate();
            }
            else
            {
                message =
                    "We changed the start of a new multiplayer game a bit and want to give you a quick overview of the new feature.\n\n" +
                    "Clients can now join while the host is in the galaxy selection screen, and they will also land there if it is their first time connecting to the currently hosted save.\n\n" +
                    "You can now click on any star to bring up the solar system preview. From there you can click on any planet to bring up its details.\n" +
                    "Note that when using GalacticScale 2 this process can take a bit longer.\n\n" +
                    "By clicking a planet while having its detail panel open you will set it as your birth planet.\n" +
                    "By clicking into outer space you will go one detail level up. Scroll to zoom in/out. Press Alt to see star names.\n\n" +
                    "Alt + ~ can open in-game chat. We hope you enjoy this new feature!";
            }

            InGamePopup.ShowInfo("The Lobby".Translate(),
                message,
                "Okay, cool :)",
                CloseLobbyInfo);
        }

        // prepare PlanetModelingManager for the use of its compute thread as we need that for the planet details view in the lobby
        PlanetModelingManager.PrepareWorks();
        // store current star id because entering the solar system details view messes up the main menu background system.
        if (MainMenuStarID == -1)
        {
            MainMenuStarID = GameMain.localStar.id;
        }

        var button = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/start-button").GetComponent<Button>();
        button.interactable = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIGalaxySelect.ApplySetting))]
    public static bool ApplySetting_Prefix(UIGalaxySelect __instance)
    {
        if (!Multiplayer.IsInMultiplayerMenu || __instance.uiCombat.active)
        {
            return true;
        }

        Multiplayer.Session.IsInLobby = false;

        if (UIVirtualStarmap_Transpiler.CustomBirthPlanet != -1)
        {
            GameMain.data.galaxy.PlanetById(UIVirtualStarmap_Transpiler.CustomBirthPlanet)?.UnloadFactory();
        }

        if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
        {
            UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);

            var gameDesc = __instance.gameDesc;
            DSPGame.StartGameSkipPrologue(gameDesc);
        }
        else
        {
            Multiplayer.Session.Network.SendPacket(new StartGameMessage());
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIGalaxySelect._OnClose))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnClose_Prefix(UIGalaxySelect __instance)
    {
        if (Multiplayer.IsInMultiplayerMenu && Multiplayer.Session.IsInLobby)
        {
            Log.Info("Closing listening Socket");
            Multiplayer.ShouldReturnToJoinMenu = false;
            Multiplayer.Session.IsInLobby = false;
            Multiplayer.LeaveGame();

            UIVirtualStarmap_Transpiler.CustomBirthStar = -1;
            UIVirtualStarmap_Transpiler.CustomBirthPlanet = -1;

            // restore main menu if needed.
            if (GameMain.localStar.id != MainMenuStarID && MainMenuStarID != -1)
            {
                GameMain.data.ArriveStar(GameMain.data.galaxy.StarById(MainMenuStarID));
            }

            // close planet detail view
            UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);
        }

        // cant check anymore if we are in multiplayer or not, so just do this without check. will not do any harm C:
        var galaxySelectRect = __instance.gameObject.GetComponent<RectTransform>();

        galaxySelectRect.Find("random-button").gameObject.SetActive(true);
        var settingGroupRect = galaxySelectRect.Find("setting-group");
        for (var i = 0; i < settingGroupRect.childCount; i++)
        {
            var childObject = settingGroupRect.GetChild(i).gameObject;
            if (childObject.name != "top-title" && childObject.name != "galaxy-seed")
                childObject.SetActive(true);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIGalaxySelect.Rerand))]
    public static void Rerand_Prefix(UIGalaxySelect __instance)
    {
        UIVirtualStarmap_Transpiler.CustomBirthStar = -1;
        UIVirtualStarmap_Transpiler.CustomBirthPlanet = -1;
        __instance.startButtonText.text = "开始游戏".Translate();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIGalaxySelect.SetStarmapGalaxy))]
    public static void SetStarmapGalaxy_Prefix()
    {
        if (!Multiplayer.IsInMultiplayerMenu || !Multiplayer.Session.LocalPlayer.IsClient)
        {
            return;
        }

        UIVirtualStarmap_Transpiler.CustomBirthStar = -1;
        UIVirtualStarmap_Transpiler.CustomBirthPlanet = -1;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGalaxySelect.UpdateParametersUIDisplay))]
    public static void UpdateParametersUIDisplay_Postfix(UIGalaxySelect __instance)
    {
        if (!Multiplayer.IsInMultiplayerMenu || !Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        // syncing players are those who have not loaded into the game yet, so they might still be in the lobby. they need to check if this packet is relevant for them in the corresponding handler.
        // just remembered others cant be in game anyways when host ist still in lobby >.>
        var packet = new LobbyUpdateValues(__instance.gameDesc.galaxyAlgo, __instance.gameDesc.galaxySeed,
            __instance.gameDesc.starCount, __instance.gameDesc.resourceMultiplier,
            __instance.gameDesc.isSandboxMode, __instance.gameDesc.isPeaceMode, __instance.gameDesc.combatSettings);

        var server = Multiplayer.Session.Server;
        var players = server.Players;

        server.SendToPlayers(players.Syncing, packet);
        server.SendToPlayers(players.Pending, packet);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIGalaxySelect._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Postfix(UIGalaxySelect __instance)
    {
        if (!Multiplayer.IsInMultiplayerMenu || !Multiplayer.IsActive || !Multiplayer.Session.IsInLobby)
        {
            return;
        }

        // as we need to load and generate planets for the detail view in the lobby, update the loading process here
        PlanetModelingManager.ModelingPlanetCoroutine();
        UIRoot.instance.uiGame.planetDetail._OnUpdate();
        if (Input.mouseScrollDelta.y == 0)
        {
            return;
        }

        // zoom in/out when scrolling
        var delta = (Input.mouseScrollDelta.y < 0 ? 1f : -1f) * (VFInput.shift ? 1f : 0.1f);
        __instance.cameraPoser.distRatio += delta;
    }

    private static void CloseLobbyInfo()
    {
        InGamePopup.FadeOut();
        Config.Options.ShowLobbyHints = false;
        Config.SaveOptions();
    }
}
