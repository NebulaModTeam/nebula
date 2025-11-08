#region

using NebulaModel.DataStructures.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class XConsoleCommandHandler : IChatCommandHandler
{
    private readonly XConsole xConsole = new();

    public void Execute(ChatService chatService, string[] parameters)
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
            chatService.AddMessage(output, ChatMessageType.CommandErrorMessage);
        }
        else
        {
            chatService.AddMessage(xConsole.consoleText, ChatMessageType.CommandOutputMessage);
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
