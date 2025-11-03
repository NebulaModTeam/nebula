#region

using System.Collections.Generic;
using System.Linq;
using NebulaModel.Logger;
using NebulaWorld.Chat.Commands;

#endregion

namespace NebulaWorld.Chat;

/// <summary>
/// A static registry for managing and accessing chat commands and their handlers.
/// </summary>
public static class ChatCommandRegistry
{
    /// <summary>
    /// The constant prefix required to identify a chat command.
    /// </summary>
    public const string CommandPrefix = "/";

    public static Dictionary<ChatCommandKey, IChatCommandHandler> Commands => commands;

    private static readonly Dictionary<ChatCommandKey, IChatCommandHandler> commands = new();

    static ChatCommandRegistry()
    {
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

    /// <summary>
    /// Registers a new chat command and its handler.
    /// </summary>
    /// <param name="commandName">The primary name of the command (case-insensitive).</param>
    /// <param name="commandHandlerHandler">The handler logic for the command.</param>
    /// <param name="aliases">Optional alternate names (aliases) for the command.</param>
    /// <returns><c>true</c> if the command was successfully registered; <c>false</c> if the handler is null or the name/alias is already registered.</returns>
    public static bool RegisterCommand(string commandName, IChatCommandHandler commandHandlerHandler, params string[] aliases)
    {
        if (commandHandlerHandler == null)
        {
            return false;
        }
        if (NameOrAliasRegistered(commandName, aliases))
        {
            Log.Debug($"Can't register command, because command for {commandName} was already registered!");
            return false;
        }

        Log.Debug($"Registering command handler for {commandName}");
        commands.Add(new ChatCommandKey(commandName, aliases), commandHandlerHandler);
        return true;
    }

    /// <summary>
    /// Unregisters a chat command by its primary name.
    /// </summary>
    /// <param name="commandName">The primary name of the command to unregister.</param>
    /// <returns><c>true</c> if the command was successfully unregistered; <c>false</c> if the command name was not found.</returns>
    public static bool UnregisterCommand(string commandName)
    {
        var chatCommandKey = commands.Keys
            .FirstOrDefault(command => command.Name == commandName);

        if (chatCommandKey == null)
        {
            Log.Debug($"Command name {commandName} is not registered!");
            return false;
        }
        Log.Debug($"Unregistering command handler for {commandName}");
        commands.Remove(chatCommandKey);
        return true;
    }

    /// <summary>
    /// Retrieves the command handler associated with a given command name or alias.
    /// </summary>
    /// <param name="commandOrAlias">The command name or alias to look up (case-insensitive).</param>
    /// <returns>The <see cref="IChatCommandHandler"/> if found; otherwise, <c>null</c>.</returns>
    public static IChatCommandHandler GetCommandHandler(string commandOrAlias)
    {
        var chatCommandKey = commands.Keys
            .FirstOrDefault(command => command.RespondsTo(commandOrAlias.ToLowerInvariant()));
        return chatCommandKey != null ? commands[chatCommandKey] : null;
    }

    /// <summary>
    /// Retrieves the aliases for a given command name or alias.
    /// </summary>
    /// <param name="commandOrAlias">The command name or alias to look up (case-insensitive).</param>
    /// <returns>An enumeration of all aliases for the command, or <c>null</c> if the command is not registered.</returns>
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

/// <summary>
/// Represents the key information for a chat command, including its primary name and aliases.
/// </summary>
public class ChatCommandKey(string commandName, IEnumerable<string> aliases)
{
    /// <summary>
    /// The set of all aliases (alternate names) for the command.
    /// </summary>
    public readonly HashSet<string> Aliases = [.. aliases];

    /// <summary>
    /// The primary name of the command.
    /// </summary>
    public readonly string Name = commandName;

    /// <summary>
    /// Checks if the command or any of its aliases match the given name/alias.
    /// </summary>
    /// <param name="nameOrAlias">The name or alias to check against (case-sensitive as stored).</param>
    /// <returns><c>true</c> if the command name or one of its aliases matches; otherwise, <c>false</c>.</returns>
    public bool RespondsTo(string nameOrAlias)
    {
        return Name == nameOrAlias || Aliases.Contains(nameOrAlias);
    }
}
