using NebulaModel.DataStructures;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class PingCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            window.SendLocalChatMessage("Pong", ChatMessageType.CommandOutputMessage);
        }

        public string GetDescription()
        {
            return "Test command";
        }

        public string GetUsage()
        {
            return "";
        }
    }
}