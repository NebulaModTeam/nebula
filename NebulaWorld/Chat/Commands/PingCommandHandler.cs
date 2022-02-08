using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class PingCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            window.SendLocalChatMessage("Pong", ChatMessageType.CommandOutputMessage);
        }

        public string GetUsage()
        {
            return $"{ChatCommandRegistry.CommandPrefix}ping - test command";
        }
    }
}