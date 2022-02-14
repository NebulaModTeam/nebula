using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NebulaWorld.Chat.Commands
{
    public class HelpCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            string output;
            if (parameters.Length > 0)
            {
                string commandName = parameters[0];

                output = GetCommandDetailsOutput(commandName);
                if (!output.Equals(""))
                {
                    window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
                }
                else
                {
                    output = $"Command {FullCommandName(commandName)} was not found! Use {FullCommandName("help")} to get list of known commands.";
                    window.SendLocalChatMessage(output, ChatMessageType.CommandErrorMessage);
                }
                return;
            }
            
            output = GetCommandListOutput();
            window.SendLocalChatMessage(output, ChatMessageType.CommandOutputMessage);
        }

        private static string GetCommandDetailsOutput(string commandName)
        {
            if (commandName.Equals("")) return "";
            
            IChatCommandHandler handler = ChatCommandRegistry.GetCommandHandler(commandName);
            if (handler == null) return "";
            
            string aliasList = ChatCommandRegistry.GetCommandAliases(commandName).Join(FullCommandName);

            StringBuilder sb = new StringBuilder();
            sb.Append($"Command {commandName} - {handler.GetDescription()}\n");
            sb.Append($"Aliases: {aliasList}\n");
            sb.Append($"Usage: {FullCommandName(commandName)} {handler.GetUsage()}");
            return sb.ToString();
        }
        
        private static string GetCommandListOutput()
        {
            StringBuilder sb = new StringBuilder("Known commands:");

            foreach (var kv in ChatCommandRegistry.commands)
            {
                sb.Append($"\n  {FullCommandName(kv.Key.Name)}");

                if (kv.Key.Aliases.Count > 0)
                {
                    string aliasList = kv.Key.Aliases.Join(FullCommandName, " ");
                    sb.Append($" ({aliasList})");
                }

                sb.Append($" - {kv.Value.GetDescription()}");
            }

            sb.Append($"\nFor detailed information about command use {FullCommandName("help")} <command name>");

            string output = sb.ToString();
            return output;
        }

        private static string FullCommandName(string name)
        {
            return ChatCommandRegistry.CommandPrefix + name;
        }
        
        public string GetDescription()
        {
            return "Get list of existing commands and their usage";
        }
        
        public string GetUsage()
        {
            return "[command name]";
        }
    }
}