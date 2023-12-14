#region

using System.Text;
using HarmonyLib;
using NebulaModel.DataStructures.Chat;
using NebulaWorld.MonoBehaviours.Local.Chat;

#endregion

namespace NebulaWorld.Chat.Commands;

public class HelpCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        string output;
        if (parameters.Length > 0)
        {
            var commandName = parameters[0];

            output = GetCommandDetailsOutput(commandName);
            if (!output.Equals(""))
            {
                window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
            }
            else
            {
                output = string.Format("Command {0} was not found! Use /help to get list of known commands.".Translate(),
                    FullCommandName(commandName));
                window.SendLocalChatMessage(output, ChatMessageType.CommandErrorMessage);
            }
            return;
        }

        output = GetCommandListOutput();
        window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
    }

    public string GetDescription()
    {
        return "Get list of existing commands and their usage".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "[command name]" };
    }

    private static string GetCommandDetailsOutput(string commandName)
    {
        if (commandName.Equals(""))
        {
            return "";
        }

        var handler = ChatCommandRegistry.GetCommandHandler(commandName);
        if (handler == null)
        {
            return "";
        }

        var aliasList = ChatCommandRegistry.GetCommandAliases(commandName).Join(FullCommandName);

        var sb = new StringBuilder();
        sb.Append("Command ".Translate()).Append(commandName).Append(" - ").Append(handler.GetDescription()).Append('\n');
        sb.Append("Aliases: ".Translate()).Append(aliasList).Append('\n');
        foreach (var usage in handler.GetUsage())
        {
            sb.Append("Usage: ".Translate()).Append(FullCommandName(commandName)).Append(' ').Append(usage).Append('\n');
        }
        return sb.ToString();
    }

    private static string GetCommandListOutput()
    {
        var sb = new StringBuilder("Known commands:".Translate());

        foreach (var kv in ChatCommandRegistry.commands)
        {
            sb.Append($"\n  {FullCommandName(kv.Key.Name)}");

            if (kv.Key.Aliases.Count > 0)
            {
                var aliasList = kv.Key.Aliases.Join(FullCommandName, " ");
                sb.Append($" ({aliasList})");
            }

            sb.Append($" - {kv.Value.GetDescription()}");
        }

        sb.Append('\n').Append("For detailed information about command use /help <command name>".Translate());

        var output = sb.ToString();
        return output;
    }

    private static string FullCommandName(string name)
    {
        return ChatCommandRegistry.CommandPrefix + name;
    }
}
