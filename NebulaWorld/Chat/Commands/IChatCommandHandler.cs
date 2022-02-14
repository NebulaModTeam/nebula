using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public interface IChatCommandHandler
    {
        void Execute(ChatWindow window, string[] parameters);
        string GetDescription();
    }
}