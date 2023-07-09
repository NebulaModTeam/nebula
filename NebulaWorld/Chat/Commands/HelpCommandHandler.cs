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
                    output = string.Format("Command {0} was not found! Use /help to get list of known commands.".Translate(), FullCommandName(commandName));
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
            sb.Append("Command ".Translate()).Append(commandName).Append(" - ").Append(handler.GetDescription()).Append('\n');
            sb.Append("Aliases: ".Translate()).Append(aliasList).Append('\n');
            foreach (string usage in handler.GetUsage())
            {
                sb.Append("Usage: ".Translate()).Append(FullCommandName(commandName)).Append(' ').Append(usage).Append('\n');
            }
            return sb.ToString();
        }
        
        private static string GetCommandListOutput()
        {
            StringBuilder sb = new StringBuilder("Known commands:".Translate());

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

            sb.Append('\n').Append("For detailed information about command use /help <command name>".Translate());

            string output = sb.ToString();
            return output;
        }

        private static string FullCommandName(string name)
        {
            return ChatCommandRegistry.CommandPrefix + name;
        }
        
        public string GetDescription()
        {
            return "Get list of existing commands and their usage".Translate();
        }
        
        public string[] GetUsage()
        {
            return new string[] { "[command name]" };
        }
    }
}