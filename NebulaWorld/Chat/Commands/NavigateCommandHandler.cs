#region

using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class NavigateCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length == 0) { return; }

        var isNumeric = int.TryParse(parameters[0], out var astroID);
        if (isNumeric)
        {
            if (GameMain.galaxy.PlanetById(astroID) != null || GameMain.galaxy.StarById(astroID) != null)
            {
                GameMain.mainPlayer.navigation.indicatorAstroId = astroID;

                var planet = GameMain.galaxy.PlanetById(astroID);
                var star = GameMain.galaxy.StarById(astroID);
                window.SendLocalChatMessage(
                    "Starting navigation to ".Translate() + (planet != null ? planet.displayName : star.displayName),
                    ChatMessageType.CommandOutputMessage);
                return;
            }
        }
        else
        {
            switch (parameters[0])
            {
                case "clear":
                case "c":
                    GameMain.mainPlayer.navigation.indicatorAstroId = 0;
                    window.SendLocalChatMessage("navigation cleared".Translate(), ChatMessageType.CommandOutputMessage);
                    return;
                case "player":
                case "p":
                    {
                        isNumeric = ushort.TryParse(parameters[1], out var playerId);

                        if (isNumeric)
                        {
                            // assume its a player id
                            using (Multiplayer.Session.World.GetRemotePlayersModels(
                                       out var remotePlayersModels))
                            {
                                foreach (var model in remotePlayersModels.Where(model => model.Value.Movement.PlayerID == playerId))
                                {
                                    Multiplayer.Session.Gizmos.SetIndicatorPlayerId(playerId);
                                    window.SendLocalChatMessage(
                                        "Starting navigation to ".Translate() + model.Value.Movement.Username,
                                        ChatMessageType.CommandOutputMessage);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            // assume its a player name
                            using (Multiplayer.Session.World.GetRemotePlayersModels(
                                       out var remotePlayersModels))
                            {
                                foreach (var model in
                                         remotePlayersModels.Where(model => model.Value.Movement.Username == parameters[1]))
                                {
                                    Multiplayer.Session.Gizmos.SetIndicatorPlayerId(model.Value.Movement.PlayerID);
                                    window.SendLocalChatMessage(
                                        "Starting navigation to ".Translate() + model.Value.Movement.Username,
                                        ChatMessageType.CommandOutputMessage);
                                    return;
                                }
                            }
                        }
                        break;
                    }
            }

            // try to find star or planet with that id
            foreach (var star in GameMain.galaxy.stars)
            {
                if (star.displayName == parameters[0])
                {
                    GameMain.mainPlayer.navigation.indicatorAstroId = star.id;
                    return;
                }
                foreach (var planet in star.planets)
                {
                    if (planet.displayName != parameters[0])
                    {
                        continue;
                    }
                    GameMain.mainPlayer.navigation.indicatorAstroId = planet.id;
                    window.SendLocalChatMessage("Starting navigation to ".Translate() + planet.displayName,
                        ChatMessageType.CommandOutputMessage);
                    return;
                }
            }
        }

        window.SendLocalChatMessage("Failed to start navigation, please check your input.".Translate(),
            ChatMessageType.CommandErrorMessage);
    }

    public string GetDescription()
    {
        return "Start navigating to a specified destination".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "<planet name | planet id | star name | star id | clear>", "player <player id | player name>" };
    }
}
