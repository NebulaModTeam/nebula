#region

using NebulaModel;
using NebulaModel.Networking;
using NebulaWorld.SocialIntegration;
using UnityEngine;

#endregion

namespace NebulaWorld;

public static class Multiplayer
{
    public static MultiplayerSession Session { get; set; }

    public static bool IsActive => Session != null;

    public static bool IsLeavingGame { get; set; }
    public static bool ShouldReturnToJoinMenu { get; set; }

    public static bool IsInMultiplayerMenu { get; set; }

    public static bool IsDedicated { get; set; }

    public static void HostGame(IServer server)
    {
        IsLeavingGame = false;

        Session = new MultiplayerSession(server);
        Session.Server!.Start();
    }

    public static void JoinGame(IClient client)
    {
        IsLeavingGame = false;

        Session = new MultiplayerSession(client);
        Session.Client!.Start();
    }

    public static void LeaveGame()
    {
        IsLeavingGame = true;

        var wasGameLoaded = Session?.IsGameLoaded ?? false;

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
        else if (ShouldReturnToJoinMenu)
        {
            var overlayCanvasGo = GameObject.Find("Overlay Canvas");
            var multiplayerMenu = overlayCanvasGo.transform.Find("Nebula - Multiplayer Menu");
            multiplayerMenu.gameObject.SetActive(true);
        }
        DiscordManager.UpdateRichPresence(string.Empty, DiscordManager.CreateSecret(), updateTimestamp: true);
    }
}
