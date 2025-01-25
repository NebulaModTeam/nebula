using System;
using System.Linq;
using NebulaModel;
using NebulaModel.Logger;
using NebulaWorld.Combat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;
using Object = System.Object;

namespace NebulaWorld.UIPlayerList
{
    public class UIPlayerWindow : MonoBehaviour
    {
        private const string WindowName = "";

        private Rect windowSize = new(10f, 10f, 1100f, 600f);
        private Vector2 playerListScrollPosition = Vector2.zero;

        private readonly Object _lockable = new();

        private bool _windowVisible;
        private ChatWindow _chatWindow;

        public void OnInit()
        {
            var parent = UIRoot.instance.uiGame.inventoryWindow.transform.parent;
            var chatGo = parent.Find("Chat Window") ? parent.Find("Chat Window").gameObject : null;

            if (chatGo != null)
            {
                _chatWindow = chatGo.transform.GetComponentInChildren<ChatWindow>();
            }
        }

        public void Update()
        {
            var hasModifier = Config.Options.PlayerListHotkey.Modifiers.Any();

            _windowVisible = false;
            if (Input.GetKey(Config.Options.PlayerListHotkey.MainKey))
            {
                if (Config.Options.PlayerListHotkey.Modifiers.All(Input.GetKey))
                {
                    // If we have no modifier but a modifier is pressed, do not progress
                    if (!hasModifier && Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftControl) ||
                        Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.RightControl)) return;

                    _windowVisible = true;
                }
            }
        }

        public void OnGUI()
        {
            if (!Multiplayer.IsActive) return;
            if (Multiplayer.Session.IsDedicated) return;

            try
            {
                if (!_windowVisible ||
                    IsChatWindowActive() ||
                    UIRoot.instance.uiGame.techTree.active ||
                    UIRoot.instance.uiGame.escMenu.active ||
                    UIRoot.instance.uiGame.dysonEditor.active)
                    return;

                windowSize = GUI.Window(6245814, windowSize, WindowHandler, WindowName, UIStyles.DialogStyles.WindowBackgroundStyle());
                windowSize.x = (int)(Screen.width * 0.5f - windowSize.width * 0.5f);
                windowSize.y = (int)(Screen.height * 0.5f - windowSize.height * 0.5f);
            }
            catch (Exception ex)
            {
                Log.Error("Error in UIPlayerWindow OnGUI");
                Log.Error(ex);
            }
        }

        public void WindowHandler(int id)
        {
            //used to track how many times a GUI element needs to be recycled in the event
            // that an exception is caught
            var areaBegins = 0;
            var horizontalBegins = 0;
            var verticalBegins = 0;
            var scrollViewBegins = 0;

            var localPlayer = GameMain.mainPlayer;

            try
            {
                GUILayout.BeginArea(new Rect(5f, 20f, windowSize.width - 10f, windowSize.height - 55f)); areaBegins++;
                GUILayout.BeginHorizontal(); horizontalBegins++;
                GUILayout.Space(2);
                GUILayout.Label("Online Players", UIStyles.LabelStyles.HeaderLabelStyle, new GUILayoutOption[] { GUILayout.ExpandHeight(false), GUILayout.ExpandWidth(true) });
                GUILayout.EndHorizontal(); horizontalBegins--;


                // If the player is alone in their save, then do not draw the scoreboard proper
                if (AmIAlone())
                {
                    GUILayout.Label("It's Just You", UIStyles.LabelStyles.CenterLabelLarge, new GUILayoutOption[] { GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true) });
                    GUILayout.EndArea();
                    return;
                }

                GUILayout.Space(20f);
                GUILayout.BeginVertical(); verticalBegins++;

                // Headers
                GUILayout.BeginHorizontal(); horizontalBegins++;
                GUILayout.Label("Name", UIStyles.LabelStyles.RowHeaderLabelsStyle, GUILayout.Width(300));
                GUILayout.Label("Location", UIStyles.LabelStyles.RowHeaderLabelsStyle, GUILayout.Width(600));
                GUILayout.Label("Distance", UIStyles.LabelStyles.RowHeaderLabelsStyle, GUILayout.Width(100));
                GUILayout.EndHorizontal(); horizontalBegins--;

                // Horizontal bar
                GUILayout.BeginHorizontal(); horizontalBegins++;
                GUILayout.Box("", GUI.skin.horizontalSlider, UIStyles.BoxStyles.HorizontalSliderStyle);
                GUILayout.EndHorizontal(); horizontalBegins--;

                GUILayout.Space(10f);

                playerListScrollPosition = GUILayout.BeginScrollView(playerListScrollPosition,
                    GUILayout.Width(windowSize.width - 10), GUILayout.ExpandHeight(true)); scrollViewBegins++;

                lock (_lockable)
                {
                    using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
                    {
                        foreach (var entry in remotePlayersModels)
                        {
                            var player = entry.Value;
                            var pName = player.Username;
                            var pDistanceFromLocalPlayer = "";
                            var pLocation = "";

                            PlanetData pPlanet = null;
                            if (Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(player.PlayerId, out var index))
                            {
                                pPlanet = GameMain.galaxy.PlanetById(Multiplayer.Session.Combat.Players[index].planetId);
                            }

                            if (pPlanet != null)
                                pLocation = $"{pPlanet.displayName}";

                            if (pPlanet == null)
                                pLocation = "In Space";

                            var pPosition = player.PlayerTransform.position;
                            var distance = Vector3.Distance(localPlayer.position, pPosition);

                            if (distance < 10000)
                            {
                                pDistanceFromLocalPlayer = $"{distance:0.00} m";
                            }
                            else if (distance < 600000.0)
                            {
                                pDistanceFromLocalPlayer = $"{(distance / 40000):0.00} AU";
                            }
                            else
                            {
                                pDistanceFromLocalPlayer = $"{(distance / 2400000.0):0.00} LY";
                            }

                            GUILayout.BeginHorizontal(); horizontalBegins++;
                            GUILayout.Label(pName, UIStyles.LabelStyles.RowLabelStyle, GUILayout.Width(300));
                            GUILayout.Label(pLocation, UIStyles.LabelStyles.RowLabelStyle, GUILayout.Width(600));
                            GUILayout.Label(pDistanceFromLocalPlayer, UIStyles.LabelStyles.RowLabelStyle,
                                GUILayout.Width(100));
                            GUILayout.EndHorizontal(); horizontalBegins--;
                            GUILayout.Space(10f);
                        }
                    }
                }
                GUILayout.EndScrollView(); scrollViewBegins--;
                GUILayout.EndVertical(); verticalBegins--;
                GUILayout.EndArea(); areaBegins--;
            }
            catch (Exception e)
            {
                Log.Error("Error in UIPlayerWindow OnGUI while building the UI");
                Log.Error(e);

                for (var i = 1; i <= areaBegins; i++)
                    GUILayout.EndArea();

                for (var i = 1; i <= horizontalBegins; i++)
                    GUILayout.EndHorizontal();

                for (var i = 1; i <= verticalBegins; i++)
                    GUILayout.EndVertical();

                for (var i = 1; i <= scrollViewBegins; i++)
                    GUILayout.EndScrollView();
            }

        }

        private bool AmIAlone()
        {
            var connectedPlayerCount = 0;

            using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
            {
                connectedPlayerCount = remotePlayersModels.Count;
            }

            return connectedPlayerCount == 0;
        }

        private bool IsChatWindowActive()
        {
            return _chatWindow != null && _chatWindow.IsActive;
        }
    }
}
