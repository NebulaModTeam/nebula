using NebulaModel.DataStructures;

namespace NebulaModel.Packets.Chat;

public class PlayerDataCommandPacket
{
    public PlayerDataCommandPacket() { }

    public PlayerDataCommandPacket(string command, string message, PlayerData playerData = null)
    {
        Command = command;
        Message = message;
        PlayerData = playerData;
    }

    public string Command { get; set; }
    public string Message { get; set; }
    public PlayerData PlayerData { get; set; }
}
