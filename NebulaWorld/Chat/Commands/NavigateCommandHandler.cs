using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using System.Collections.Generic;

namespace NebulaWorld.Chat.Commands
{
    public class NavigateCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if(parameters.Length == 0) { return; } 

            bool isNumeric = int.TryParse(parameters[0], out var astroID);
            if (isNumeric)
            {
                if(GameMain.galaxy.PlanetById(astroID) != null || GameMain.galaxy.StarById(astroID) != null)
                {
                    GameMain.mainPlayer.navigation.indicatorAstroId = astroID;

                    PlanetData planet = GameMain.galaxy.PlanetById(astroID);
                    StarData star = GameMain.galaxy.StarById(astroID);
                    window.SendLocalChatMessage("Starting navigation to " + ((planet != null) ? planet.displayName : star.displayName), ChatMessageType.CommandOutputMessage);
                    return;
                }
            }
            else
            {
                if(parameters[0] == "clear" || parameters[0] == "c")
                {
                    GameMain.mainPlayer.navigation.indicatorAstroId = 0;
                    window.SendLocalChatMessage("navigation cleared", ChatMessageType.CommandOutputMessage);
                    return;
                }
                if(parameters[0] == "player" || parameters[0] == "p")
                {
                    isNumeric = int.TryParse(parameters[1], out var ID);

                    if (isNumeric)
                    {
                        // assume its a player id
                        using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
                        {
                            foreach (KeyValuePair<ushort, RemotePlayerModel> model in remotePlayersModels)
                            {
                                if (model.Value.Movement.PlayerID == ID)
                                {
                                    // handle indicator position update in RemotePlayerMovement.cs
                                    GameMain.mainPlayer.navigation.indicatorAstroId = 100000 + ID;
                                    window.SendLocalChatMessage("Starting navigation to " + model.Value.Movement.Username, ChatMessageType.CommandOutputMessage);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        // assume its a player name
                        using (Multiplayer.Session.World.GetRemotePlayersModels(out Dictionary<ushort, RemotePlayerModel> remotePlayersModels))
                        {
                            foreach (KeyValuePair<ushort, RemotePlayerModel> model in remotePlayersModels)
                            {
                                if (model.Value.Movement.Username == parameters[1])
                                {
                                    // handle indicator position update in RemotePlayerMovement.cs
                                    GameMain.mainPlayer.navigation.indicatorAstroId = 100000 + model.Value.Movement.PlayerID;
                                    window.SendLocalChatMessage("Starting navigation to " + model.Value.Movement.Username, ChatMessageType.CommandOutputMessage);
                                    return;
                                }
                            }
                        }
                    }
                }

                // try to find star or planet with that id
                foreach(StarData star in GameMain.galaxy.stars)
                {
                    if(star.displayName == parameters[0])
                    {
                        GameMain.mainPlayer.navigation.indicatorAstroId = star.id;
                        return;
                    }
                    foreach(PlanetData planet in star.planets)
                    {
                        if(planet.displayName == parameters[0])
                        {
                            GameMain.mainPlayer.navigation.indicatorAstroId = planet.id;
                            window.SendLocalChatMessage("Starting navigation to " + planet.displayName, ChatMessageType.CommandOutputMessage);
                            return;
                        }
                    }
                }
            }

            window.SendLocalChatMessage("Failed to start navigation, please check your input.", ChatMessageType.CommandOutputMessage);
        }

        public string GetDescription()
        {
            return "Start navigating to a specified destination";
        }
        public string GetUsage()
        {
            return "[planet name | planet id | star name | star id | clear]\r\nUsage: /n player [player id | playr name]";
        }
    }
}
