using NebulaModel;
using UnityEngine;

namespace NebulaWorld
{
    public static class Multiplayer
    {
        public static MultiplayerSession Session { get; private set; }

        public static bool IsActive => Session != null;

        public static bool IsLeavingGame { get; set; }

        public static bool IsInMultiplayerMenu { get; set; }


        public static void HostGame(NetworkProvider server)
        {
            IsLeavingGame = false;

            Session = new MultiplayerSession(server);
            ((NetworkProvider)Session.Network).Start();
        }

        public static void JoinGame(NetworkProvider client)
        {
            IsLeavingGame = false;

            Session = new MultiplayerSession(client);
            ((NetworkProvider)Session.Network).Start();
        }

        public static void LeaveGame()
        {
            IsLeavingGame = true;

            bool wasGameLoaded = Session?.IsGameLoaded ?? false;

            if (wasGameLoaded)
            {
                Session.World.HidePingIndicator();
            }

            Session?.Dispose();
            Session = null;

            if (wasGameLoaded)
            {
                if (!UIRoot.instance.backToMainMenu)
                {
                    UIRoot.instance.backToMainMenu = true;
                    DSPGame.EndGame();
                }
            }
            else
            {
                GameObject overlayCanvasGo = GameObject.Find("Overlay Canvas");
                Transform multiplayerMenu = overlayCanvasGo.transform.Find("Nebula - Multiplayer Menu");
                multiplayerMenu.gameObject.SetActive(true);
            }
        }
    }
}
