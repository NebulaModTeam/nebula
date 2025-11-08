#region

using System.Linq;
using NebulaModel.DataStructures.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class SystemCommandHandler : IChatCommandHandler
{
    public void Execute(ChatService chatService, string[] parameters)
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

        var result = GameMain.galaxy.stars.Where(star => star.displayName == input).Aggregate("", (current1, star) => star.planets.Aggregate(current1, (current, planet) => current + planet.displayName + " (" + planet.id + ")" + (planet.orbitAroundPlanet != null ? " (moon)".Translate() : "") + "\r\n"));

        if (result == "")
        {
            result = string.Format("Could not find given star '{0}'".Translate(), input);
        }

        chatService.AddMessage(result, ChatMessageType.CommandOutputMessage);
    }

    public string GetDescription()
    {
        return "List planets in a system".Translate();
    }

    public string[] GetUsage()
    {
        return ["[star name]"];
    }
}
