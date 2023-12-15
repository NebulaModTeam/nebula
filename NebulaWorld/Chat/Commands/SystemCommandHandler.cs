#region

using System.Linq;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class SystemCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
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

        var resp = GameMain.galaxy.stars.Where(star => star.displayName == input).Aggregate("", (current1, star) => star.planets.Aggregate(current1, (current, planet) => current + planet.displayName + " (" + planet.id + ")" + (planet.orbitAroundPlanet != null ? " (moon)".Translate() : "") + "\r\n"));

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
