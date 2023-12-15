#region

using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Session;

public class PlayerJoining
{
    public PlayerJoining() { }

    public PlayerJoining(PlayerData playerData, ushort numPlayers)
    {
        PlayerData = playerData;
        NumPlayers = numPlayers;
    }

    public PlayerData PlayerData { get; set; }
    public ushort NumPlayers { get; set; }
}
