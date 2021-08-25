using NebulaModel;
using UnityEngine;

namespace NebulaWorld
{
    public static class Multiplayer
    {
        public static MultiplayerSession Session { get; private set; }

        public static bool IsActive => Session != null;
        public static bool IsLeavingGame { get; set; }

        public static void HostGame(NetworkProvider server)
        {
            IsLeavingGame = false;

            Session = new MultiplayerSession(server);
            Session.Network.Start();
        }

        public static void JoinGame(NetworkProvider client)
        {
            IsLeavingGame = false;

            Session = new MultiplayerSession(client);
            Session.Network.Start();
        }

        public static void LeaveGame()
        {
            IsLeavingGame = true;

            bool wasGameLoaded = Session?.IsGameLoaded ?? false;

            // TODO: MAYBE WE SHOULD DO SOMETHING LIKE THIS INSTEAD:
            /*
             * Session.Network.Stop(() => {
             *      This would be called once the actual socket.close event is fired
             *      So here we could dispose of the Session
             *      And do the transition back to the main menu
             * })
             *
             */

            Session?.Dispose();
            Session = null;

            // TODO: THIS WILL PROBABLY BE CALLED BEFORE THE ACTUAL CLIENT DISCONNECT POPUP, IS IT AN ISSUE ??
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
