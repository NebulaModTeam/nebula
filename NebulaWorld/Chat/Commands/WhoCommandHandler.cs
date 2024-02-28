#region

using System.Linq;
using System.Text;
using NebulaAPI.GameState;
using NebulaModel.DataStructures.Chat;
using NebulaModel.Packets.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;
using static NebulaWorld.Chat.ChatLinks.NavigateChatLinkHandler;

#endregion

namespace NebulaWorld.Chat.Commands;

public class WhoCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (!Multiplayer.Session.LocalPlayer.IsHost)
        {
            // send a request to host
            Multiplayer.Session.Network.SendPacket(new ChatCommandWhoPacket(true, null));
        }
        else
        {
            var playerDatas = Multiplayer.Session.Server.Players.GetAllPlayerData().ToArray();
            var hostPlayer = Multiplayer.Session.LocalPlayer;
            var messageContent = BuildResultPayload(playerDatas, hostPlayer);
            window.SendLocalChatMessage(messageContent, ChatMessageType.CommandOutputMessage);
        }
    }

    public string GetDescription()
    {
        return "List all players and their locations".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "" };
    }

    public static string BuildResultPayload(IPlayerData[] allPlayers, ILocalPlayer hostPlayer)
    {
        var sb = new StringBuilder(string.Format("/who results: ({0} players)\r\n".Translate(), allPlayers.Length));
        foreach (var playerData in allPlayers)
        {
            sb.Append(BuildWhoMessageTextForPlayer(playerData, hostPlayer)).Append("\r\n");
        }

        return sb.ToString();
    }

    private static string BuildWhoMessageTextForPlayer(IPlayerData playerData, ILocalPlayer localPlayer)
    {
        var sb = new StringBuilder($"[{playerData.PlayerId}] {FormatNavigateToPlayerString(playerData.PlayerId, playerData.Username)}");
        if (localPlayer.Id == playerData.PlayerId)
        {
            sb.Append(" (host)".Translate());
        }

        string playerPlanetString = null;
        if (playerData.LocalPlanetId > 0)
        {
            var playerPlanet = GameMain.galaxy.PlanetById(playerData.LocalPlanetId);
            if (playerPlanet != null)
            {
                playerPlanetString = ", " + playerPlanet.name;
            }
        }

        string playerSystemString = null;
        if (playerData.LocalStarId > 0)
        {
            var starData = GameMain.galaxy.StarById(playerData.LocalStarId);
            if (starData != null)
            {
                playerSystemString = ", " + starData.name;
            }
        }


        sb.Append(!string.IsNullOrWhiteSpace(playerPlanetString) ? playerPlanetString : ", in space".Translate());

        if (!string.IsNullOrWhiteSpace(playerSystemString))
        {
            sb.Append(playerSystemString);
        }
        else
        {
            sb.Append(", at coordinates ".Translate() + playerData.UPosition);
        }

        return sb.ToString();
    }
}
