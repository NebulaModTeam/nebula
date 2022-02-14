using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;
using System.Text;

namespace NebulaWorld.Chat.Commands
{
    public class HelpCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            StringBuilder sb = new StringBuilder("Known commands:");

            foreach (var kv in ChatCommandRegistry.commands)
            {
                sb.Append($"\n {FullCommandName(kv.Key.Name)}(");
                foreach (string alias in kv.Key.Aliases)
                {
                    sb.Append($"{FullCommandName(alias)} ");
                }

                sb.Append($") - {kv.Value.GetDescription()}");
            }
            window.SendLocalChatMessage(sb.ToString(), ChatMessageType.CommandOutputMessage);
        }

        private static string FullCommandName(string name)
        {
            return ChatCommandRegistry.CommandPrefix + name;
        }
        
        public string GetDescription()
        {
            return "Get list of existing commands and their usage";
        }
    }
}