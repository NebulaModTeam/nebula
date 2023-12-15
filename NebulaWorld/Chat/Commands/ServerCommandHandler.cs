#region

using NebulaModel.Packets.Chat;
using NebulaModel.Utils;
using NebulaWorld.MonoBehaviours.Local.Chat;

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
        switch (parameters[0])
        {
            case "login":
                {
                    var password = parameters.Length > 1 ? parameters[1] : "";
                    var playerData = Multiplayer.Session.LocalPlayer.Data;
                    var salt = playerData.Username + playerData.PlayerId;
                    var hash = CryptoUtils.Hash(password + salt);
                    Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.Login, hash));
                    break;
                }
            case "list":
                {
                    var saveNum = parameters.Length > 1 ? parameters[1] : "";
                    Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerList,
                        saveNum));
                    break;
                }
            case "save":
                {
                    var saveName = parameters.Length > 1 ? parameters[1] : "";
                    Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerSave,
                        saveName));
                    break;
                }
            case "load" when parameters.Length < 2:
                throw new ChatCommandUsageException("Need to specifiy a save!");
            case "load":
                {
                    var saveName = parameters.Length > 1 ? parameters[1] : "";
                    Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerLoad,
                        saveName));
                    break;
                }
            case "info":
                {
                    var parameter = parameters.Length > 1 ? parameters[1] : "";
                    Multiplayer.Session.Network.SendPacket(new RemoteServerCommandPacket(RemoteServerCommand.ServerInfo,
                        parameter));
                    break;
                }
            default:
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
