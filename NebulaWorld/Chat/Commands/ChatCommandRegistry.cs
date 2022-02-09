using NebulaModel.Logger;
using System.Collections.Generic;
using System.Linq;

namespace NebulaWorld.Chat.Commands
{
    public static class ChatCommandRegistry
    {
        public const string CommandPrefix = "/";
        private static Dictionary<string, IChatCommandHandler> commands = new Dictionary<string, IChatCommandHandler>();

        public static void RegisterCommand(string commandName, IChatCommandHandler commandHandlerHandler)
        {
            if (commandHandlerHandler == null) return;
            if (commands.ContainsKey(commandName))
            {
                Log.Debug($"Can't register command, because command for {commandName} was already registered!");
                return;
            }
            
            Log.Debug($"Registering command handler for {commandName}");
            commands.Add(commandName, commandHandlerHandler);
        }
        
        public static IChatCommandHandler GetCommandHandler(string commandName)
        {
            if (commands.ContainsKey(commandName))
            {
                return commands[commandName];
            }

            return null;
        }

        public static IChatCommandHandler[] GetCommands()
        {
            return commands.Values.ToArray();
        }

        static ChatCommandRegistry()
        {
            RegisterCommand("ping", new PingCommandHandler());
            RegisterCommand("help", new HelpCommandHandler());
            RegisterCommand("who", new WhoCommandHandler());
        }
        
        
    }
}