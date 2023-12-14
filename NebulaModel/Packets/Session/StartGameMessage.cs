#region

using NebulaModel.DataStructures;

#endregion

namespace NebulaModel.Packets.Session;

public class StartGameMessage
{
    public StartGameMessage() { }

    public StartGameMessage(bool isAllowedToStart, PlayerData localPlayerData, bool syncSoil)
    {
        IsAllowedToStart = isAllowedToStart;
        LocalPlayerData = localPlayerData;
        SyncSoil = syncSoil;
    }

    public bool IsAllowedToStart { get; set; }
    public PlayerData LocalPlayerData { get; set; }
    public bool SyncSoil { get; set; }
}
