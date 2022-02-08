using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class HelpCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            IChatCommandHandler[] handlers = ChatCommandRegistry.GetCommands();
            string message = "Known commands:";

            foreach (IChatCommandHandler handler in handlers)
            {
                message += $"\n {handler.GetUsage()}";
            }
            window.SendLocalChatMessage(message, ChatMessageType.CommandOutputMessage);
        }
        
        public string GetUsage()
        {
            return $"{ChatCommandRegistry.CommandPrefix}help - get list of existing commands and their usage";
        }
    }
}