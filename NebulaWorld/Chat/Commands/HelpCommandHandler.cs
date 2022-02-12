using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using System.Text;

namespace NebulaWorld.Chat.Commands
{
    public class HelpCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            IChatCommandHandler[] handlers = ChatCommandRegistry.GetCommands();
            StringBuilder sb = new StringBuilder("Known commands:");

            foreach (IChatCommandHandler handler in handlers)
            {
                sb.Append($"\n {handler.GetUsage()}");
            }
            window.SendLocalChatMessage(sb.ToString(), ChatMessageType.CommandOutputMessage);
        }
        
        public string GetUsage()
        {
            return $"{ChatCommandRegistry.CommandPrefix}help - get list of existing commands and their usage";
        }
    }
}