using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;

namespace NebulaWorld.Chat.Commands
{
    public class ServerSaveCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            string saveName = parameters.Length > 0 ? parameters[0] : "";
            Multiplayer.Session.Network.SendPacket(new AdminCommandPacket(AdminCommand.ServerSave, saveName));
        }

        public string GetDescription()
        {
            return "Tell dedicated server to save game";
        }

        public string[] GetUsage()
        {
            return new string[] { "[saveName]" };
        }
    }
}
