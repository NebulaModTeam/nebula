using NebulaModel;

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

            Session?.Dispose();
            Session = null;

            if (!UIRoot.instance.backToMainMenu)
            {
                UIRoot.instance.backToMainMenu = true;
                DSPGame.EndGame();
            }
        }
    }
}
