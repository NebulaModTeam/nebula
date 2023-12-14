#region

using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;

#endregion

namespace NebulaWorld.Chat.Commands;

public class SystemCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        var resp = "";
        var input = "";

        if (parameters.Length == 0)
        {
            input = GameMain.localStar.displayName;
        }
        else
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                input += (i > 0 ? " " : "") + parameters[i];
            }
        }

        foreach (var star in GameMain.galaxy.stars)
        {
            if (star.displayName == input)
            {
                foreach (var planet in star.planets)
                {
                    resp += planet.displayName + " (" + planet.id + ")" +
                            (planet.orbitAroundPlanet != null ? " (moon)".Translate() : "") + "\r\n";
                }
            }
        }

        if (resp == "")
        {
            resp = string.Format("Could not find given star '{0}'".Translate(), input);
        }

        window.SendLocalChatMessage(resp, ChatMessageType.CommandOutputMessage);
    }

    public string GetDescription()
    {
        return "List planets in a system".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "[star name]" };
    }
}
