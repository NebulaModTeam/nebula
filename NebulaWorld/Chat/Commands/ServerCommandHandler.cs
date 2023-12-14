#region

using NebulaAPI;
using NebulaModel.Packets.Players;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local;

#endregion

namespace NebulaWorld.Chat.Commands;

public class ServerCommandHandler : IChatCommandHandler
{
    public void Execute(ChatWindow window, string[] parameters)
    {
        if (parameters.Length < 1)
        {
            throw new ChatCommandUsageException("Not enough arguments!".Translate());
        }
        if (parameters[0] == "login")
        {
            var password = parameters.Length > 1 ? parameters[1] : "";
            IPlayerData playerData = Multiplayer.Session.LocalPlayer.Data;
            var salt = playerData.Username + playerData.PlayerId;
            var hash = CryptoUtils.Hash(password + salt);
            Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.Login, hash));
        }
        else if (parameters[0] == "list")
        {
            var saveNum = parameters.Length > 1 ? parameters[1] : "";
            Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerList, saveNum));
        }
        else if (parameters[0] == "save")
        {
            var saveName = parameters.Length > 1 ? parameters[1] : "";
            Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerSave, saveName));
        }
        else if (parameters[0] == "load")
        {
            if (parameters.Length < 2)
            {
                throw new ChatCommandUsageException("Need to specifiy a save!");
            }
            var saveName = parameters.Length > 1 ? parameters[1] : "";
            Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerLoad, saveName));
        }
        else if (parameters[0] == "info")
        {
            var parameter = parameters.Length > 1 ? parameters[1] : "";
            Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerInfo, parameter));
        }
        else
        {
            throw new ChatCommandUsageException(
                "Unknown command! Available commands: {login, list, save, load, info}".Translate());
        }
    }

    public string GetDescription()
    {
        return "Tell remote server to save/load".Translate();
    }

    public string[] GetUsage()
    {
        return new[] { "login <password>", "list [saveNum]", "save [saveName]", "load <saveName>", "info" };
    }
}
