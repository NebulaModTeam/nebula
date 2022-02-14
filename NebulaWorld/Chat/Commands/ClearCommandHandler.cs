using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class ClearCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            window.ClearChat();
        }

        public string GetDescription()
        {
            return "Clear all chat messages (locally)";
        }
    }
}