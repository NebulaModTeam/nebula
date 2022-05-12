using NebulaModel.Logger;
using System.Collections.Generic;
using System.Linq;

namespace NebulaWorld.Chat.Commands
{
    public static class ChatCommandRegistry
    {
        public const string CommandPrefix = "/";
        public static readonly Dictionary<ChatCommandKey, IChatCommandHandler> commands = new Dictionary<ChatCommandKey, IChatCommandHandler>();

        public static void RegisterCommand(string commandName, IChatCommandHandler commandHandlerHandler, params string[] aliases)
        {
            if (commandHandlerHandler == null) return;
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
            ChatCommandKey chatCommandKey = commands.Keys
                .FirstOrDefault((command) => command.RespondsTo(commandOrAlias.ToLowerInvariant()));
            return chatCommandKey != null ? commands[chatCommandKey] : null;
        }
        
        public static string[] GetCommandAliases(string commandOrAlias)
        {
            ChatCommandKey chatCommandKey = commands.Keys
                .FirstOrDefault((command) => command.RespondsTo(commandOrAlias.ToLowerInvariant()));
            return chatCommandKey?.Aliases.ToArray();
        }
        
        private static bool NameOrAliasRegistered(string commandName, string[] aliases)
        {
            if (commands.Keys.Any(command => command.RespondsTo(commandName)))
            {
                return true;
            }

            HashSet<string> aliasesSet = new HashSet<string>(aliases);
            foreach (ChatCommandKey command in commands.Keys)
            {
                if (aliasesSet.Overlaps(command.Aliases))
                {
                    return true;
                }
            }

            return false;
        }

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
            RegisterCommand("server", new ServerSaveCommandHandler());
        }
    }

    public class ChatCommandKey
    {
        public readonly string Name;
        public readonly HashSet<string> Aliases;

        public ChatCommandKey(string commandName, string[] aliases)
        {
            Name = commandName;
            Aliases = new HashSet<string>(aliases);
        }

        public bool RespondsTo(string nameOrAlias)
        {
            return Name == nameOrAlias || Aliases.Contains(nameOrAlias);
        }
    }
}