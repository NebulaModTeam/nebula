#region

using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class XConsoleCommandHandler : IChatCommandHandler
{
    private readonly XConsole xConsole = new();

    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length <= 0)
        {
            return;
        }
        var commandText = string.Join(" ", parameters);
        xConsole.ExecuteCommand(commandText);
        var output = xConsole.consoleText;
        if (output.EndsWith("Bad command.</color>\r\n"))
        {
            output = $">> {commandText}\n" + ">> Bad command. Use /x -help to get list of known commands.".Translate();
            window.SendLocalChatMessage(output, ChatMessageType.CommandErrorMessage);
        }
        else
        {
            window.SendLocalChatMessage(xConsole.consoleText, ChatMessageType.CommandOutputMessage);
        }
        xConsole.consoleText = "";
        xConsole.history_cmds.Clear();
    }

    public string GetDescription()
    {
        return "Execute developer console command".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "[XConsole command]" };
    }
}
