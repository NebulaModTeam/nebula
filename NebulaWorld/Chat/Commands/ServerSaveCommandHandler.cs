using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using NebulaModel.Utils;
using NebulaAPI;

namespace NebulaWorld.Chat.Commands
{
    public class ServerSaveCommandHandler : IChatCommandHandler
    {
        public void Execute(ChatWindow window, string[] parameters)
        {
            if (parameters.Length < 1)
            {
                throw new ChatCommandUsageException("Not enough arguments!");
            }
            if (parameters[0] == "login")
            {
                string password = parameters.Length > 1 ? parameters[1] : "";
                IPlayerData playerData = Multiplayer.Session.LocalPlayer.Data;
                string salt = playerData.Username + playerData.PlayerId;
                string hash = CryptoUtils.Hash(password + salt);
                Multiplayer.Session.Network.SendPacket(new RemoteSaveCommandPacket(RemoteSaveCommand.Login, hash));
            }
            else if (parameters[0] == "list")
            {
                string saveNum = parameters.Length > 1 ? parameters[1] : "";
                Multiplayer.Session.Network.SendPacket(new RemoteSaveCommandPacket(RemoteSaveCommand.ServerList, saveNum));
            }
            else if (parameters[0] == "save")
            {
                string saveName = parameters.Length > 1 ? parameters[1] : "";
                Multiplayer.Session.Network.SendPacket(new RemoteSaveCommandPacket(RemoteSaveCommand.ServerSave, saveName));
            }
            else if (parameters[0] == "load")
            {
                if (parameters.Length < 2)
                {
                    throw new ChatCommandUsageException("Need to specifiy a save!");
                }
                string saveName = parameters.Length > 1 ? parameters[1] : "";
                Multiplayer.Session.Network.SendPacket(new RemoteSaveCommandPacket(RemoteSaveCommand.ServerLoad, saveName));
            }
            else
            {
                throw new ChatCommandUsageException("Unknown command! Available commands: {login, list, save, load}");
            }
        }

        public string GetDescription()
        {
            return "Tell dedicated server to save/load";
        }

        public string[] GetUsage()
        {
            return new string[] { "login <password>", "list [saveNum]" , "save [saveName]", "load <saveName>" };
        }
    }
}
