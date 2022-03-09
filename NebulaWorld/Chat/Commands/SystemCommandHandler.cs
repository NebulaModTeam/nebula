using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class SystemCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            string resp = "";
            string input = "";

            if(parameters.Length == 0)
            {
                input = GameMain.localStar.displayName;
            }
            else
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    input += ((i > 0) ? " " : "") + parameters[i];
                }
            }

            foreach (StarData star in GameMain.galaxy.stars)
            {
                if (star.displayName == input)
                {
                    foreach (PlanetData planet in star.planets)
                    {
                        resp += planet.displayName + " (" + planet.id + ")" + ((planet.orbitAroundPlanet != null) ? " (moon)" : "") + "\r\n";
                    }
                }
            }

            if (resp == "")
            {
                resp = "Could not find given star '" + input + "'";
            }

            window.SendLocalChatMessage(resp, ChatMessageType.CommandOutputMessage);
        }
        public string GetDescription()
        {
            return "List planets in a system";
        }
        public string GetUsage()
        {
            return "[star name]";
        }
    }
}
