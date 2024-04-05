#region

using System.Collections.Generic;
using System.Linq;
using NebulaModel.Logger;
using NebulaWorld.Chat.Commands;

#endregion

namespace NebulaWorld.Chat;

public static class ChatCommandRegistry
{
    public const string CommandPrefix = "/";
    public static readonly Dictionary<ChatCommandKey, IChatCommandHandler> commands = new();

    static ChatCommandRegistry()
    {
        RegisterCommand("ping", new PingCommandHandler());
        RegisterCommand("help", new HelpCommandHandler(), "h", "?");
        RegisterCommand("who", new WhoCommandHandler(), "players", "list");
        RegisterCommand("whisper", new WhisperCommandHandler(), "w", "tell", "t");
        RegisterCommand("info", new InfoCommandHandler());
        RegisterCommand("clear", new ClearCommandHandler(), "c");
        RegisterCommand("xconsole", new XConsoleCommandHandler(), "x");
        RegisterCommand("navigate", new NavigateCommandHandler(), "n");
        RegisterCommand("system", new SystemCommandHandler(), "s");
        RegisterCommand("reconnect", new ReconnectCommandHandler(), "r");
        RegisterCommand("server", new ServerCommandHandler());
        RegisterCommand("playerdata", new PlayerDataCommandHandler());
        RegisterCommand("dev", new DevCommandHandler());
    }

    private static void RegisterCommand(string commandName, IChatCommandHandler commandHandlerHandler, params string[] aliases)
    {
        if (commandHandlerHandler == null)
        {
            return;
        }
        if (NameOrAliasRegistered(commandName, aliases))
        {
            Log.Debug($"Can't register command, because command for {commandName} was already registered!");
            return;
        }

        Log.Debug($"Registering command handler for {commandName}");
        commands.Add(new ChatCommandKey(commandName, aliases), commandHandlerHandler);
    }

    public static IChatCommandHandler GetCommandHandler(string commandOrAlias)
    {
        var chatCommandKey = commands.Keys
            .FirstOrDefault(command => command.RespondsTo(commandOrAlias.ToLowerInvariant()));
        return chatCommandKey != null ? commands[chatCommandKey] : null;
    }

    public static IEnumerable<string> GetCommandAliases(string commandOrAlias)
    {
        var chatCommandKey = commands.Keys
            .FirstOrDefault(command => command.RespondsTo(commandOrAlias.ToLowerInvariant()));
        return chatCommandKey?.Aliases.ToArray();
    }

    private static bool NameOrAliasRegistered(string commandName, IEnumerable<string> aliases)
    {
        if (commands.Keys.Any(command => command.RespondsTo(commandName)))
        {
            return true;
        }

        var aliasesSet = new HashSet<string>(aliases);
        return commands.Keys.Any(command => aliasesSet.Overlaps(command.Aliases));
    }
}

public class ChatCommandKey(string commandName, IEnumerable<string> aliases)
{
    public readonly HashSet<string> Aliases = [.. aliases];
    public readonly string Name = commandName;

    public bool RespondsTo(string nameOrAlias)
    {
        return Name == nameOrAlias || Aliases.Contains(nameOrAlias);
    }
}
