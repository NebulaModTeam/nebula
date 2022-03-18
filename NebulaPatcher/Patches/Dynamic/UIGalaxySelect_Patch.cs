using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Packets.Session;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIGalaxySelect))]
    internal class UIGalaxySelect_Patch
    {
        private static int MainMenuStarID = -1;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGalaxySelect._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix(UIGalaxySelect __instance)
        {
            if(Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsClient)
            {
                RectTransform galaxySelectRect = __instance.gameObject.GetComponent<RectTransform>();

                galaxySelectRect.Find("star-count").gameObject.SetActive(false);
                galaxySelectRect.Find("resource-multiplier").gameObject.SetActive(false);
                galaxySelectRect.Find("galaxy-seed").GetComponentInChildren<InputField>().enabled = false;
                galaxySelectRect.Find("random-button").gameObject.SetActive(false);
            }
            if (Multiplayer.IsActive)
            {
                // show lobby hints if needed
                if (Config.Options.ShowLobbyHints)
                {
                    InGamePopup.ShowInfo("The Lobby",
                        "We changed the start of a new multiplayer game a bit and want to give you a quick overview of the new feature.\n\n" +
                        "Clients can now join while the host is in the galaxy selection screen, and they will also land there if it is their first time connecting to the currently hosted save.\n\n" +
                        "You can now click on any star to bring up the solar system preview. From there you can click on any planet to bring up its details.\n" +
                        "Note that when using GalacticScale 2 this process can take a bit longer.\n\n" +
                        "By clicking a planet while having its detail panel open you will set it as your birth planet.\n\n" +
                        "By clicking into outer space you will go one detail level up.\n\n" +
                        "We hope you enjoy this new feature!",
                        "Okay, cool :)",
                        CloseLobbyInfo);
                }
                // prepare PlanetModelingManager for the use of its compute thread as we need that for the planet details view in the lobby
                PlanetModelingManager.PrepareWorks();
                // store current star id because entering the solar system details view messes up the main menu background system.
                if(MainMenuStarID == -1)
                {
                    MainMenuStarID = GameMain.localStar.id;
                }
                Button button = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/start-button").GetComponent<Button>();
                button.interactable = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIGalaxySelect.EnterGame))]
        public static bool EnterGame_Prefix(UIGalaxySelect __instance)
        {
            if (Multiplayer.IsInMultiplayerMenu)
            {
                Multiplayer.Session.IsInLobby = false;

                if(UIVirtualStarmap_Transpiler.customBirthPlanet != -1)
                {
                    Debug.Log((GameMain.data.galaxy.PlanetById(UIVirtualStarmap_Transpiler.customBirthPlanet) == null) ? "null" : "not null");
                    GameMain.data.galaxy.PlanetById(UIVirtualStarmap_Transpiler.customBirthPlanet)?.UnloadFactory();
                }

                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
                {
                    UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);

                    GameDesc gameDesc = __instance.gameDesc;
                    DSPGame.StartGameSkipPrologue(gameDesc);
                }
                else
                {
                    Multiplayer.Session.Network.SendPacket(new StartGameMessage());
                }
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIGalaxySelect.CancelSelect))]
        public static void CancelSelect_Prefix(UIGalaxySelect __instance)
        {
            if (Multiplayer.IsInMultiplayerMenu && Multiplayer.Session.IsInLobby)
            {
                Log.Info($"Closing listening Socket");
                Multiplayer.ShouldReturnToJoinMenu = false;
                Multiplayer.Session.IsInLobby = false;
                Multiplayer.LeaveGame();

                UIVirtualStarmap_Transpiler.customBirthStar = -1;
                UIVirtualStarmap_Transpiler.customBirthPlanet = -1;

                // restore main menu if needed.
                if(GameMain.localStar.id != MainMenuStarID && MainMenuStarID != -1)
                {
                    GameMain.data.ArriveStar(GameMain.data.galaxy.StarById(MainMenuStarID));
                }

                // close planet detail view
                UIRoot.instance.uiGame.planetDetail.gameObject.SetActive(false);
            }

            // cant check anymore if we are in multiplayer or not, so just do this without check. will not do any harm C:
            RectTransform galaxySelectRect = __instance.gameObject.GetComponent<RectTransform>();

            galaxySelectRect.Find("star-count").gameObject.SetActive(true);
            galaxySelectRect.Find("resource-multiplier").gameObject.SetActive(true);
            galaxySelectRect.Find("galaxy-seed").GetComponentInChildren<InputField>().enabled = true;
            galaxySelectRect.Find("random-button").gameObject.SetActive(true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIGalaxySelect.Rerand))]
        public static void Rerand_Prefix()
        {
            UIVirtualStarmap_Transpiler.customBirthStar = -1;
            UIVirtualStarmap_Transpiler.customBirthPlanet = -1;
            GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/start-button/start-text").GetComponent<Text>().text = "Start Game";
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIGalaxySelect.SetStarmapGalaxy))]
        public static void SetStarmapGalaxy_Prefix()
        {
            if(Multiplayer.IsInMultiplayerMenu && Multiplayer.Session.LocalPlayer.IsClient)
            {
                UIVirtualStarmap_Transpiler.customBirthStar = -1;
                UIVirtualStarmap_Transpiler.customBirthPlanet = -1;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGalaxySelect.SetStarmapGalaxy))]
        public static void SetStarmapGalaxy_Postfix(UIGalaxySelect __instance)
        {
            if(Multiplayer.IsInMultiplayerMenu && Multiplayer.Session.LocalPlayer.IsHost)
            {
                // syncing players are those who have not loaded into the game yet, so they might still be in the lobby. they need to check if this packet is relevant for them in the corresponding handler.
                // just remembered others cant be in game anyways when host ist still in lobby >.>
                using (Multiplayer.Session.Network.PlayerManager.GetSyncingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers))
                {
                    foreach(KeyValuePair<INebulaConnection, INebulaPlayer> entry in syncingPlayers)
                    {
                        entry.Key.SendPacket(new LobbyUpdateValues(__instance.gameDesc.galaxyAlgo, __instance.gameDesc.galaxySeed, __instance.gameDesc.starCount, __instance.gameDesc.resourceMultiplier));
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIGalaxySelect._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnUpdate_Postfix()
        {
            if (Multiplayer.IsInMultiplayerMenu)
            {
                // as we need to load and generate planets for the detail view in the lobby, update the loading process here
                PlanetModelingManager.ModelingPlanetCoroutine();
                UIRoot.instance.uiGame.planetDetail._OnUpdate();
            }
        }

        private static void CloseLobbyInfo()
        {
            InGamePopup.FadeOut();
            Config.Options.ShowLobbyHints = false;
            Config.SaveOptions();
        }
    }
}
