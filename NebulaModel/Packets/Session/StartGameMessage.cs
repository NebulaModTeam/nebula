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

    public bool IsAllowedToStart { get; }
    public PlayerData LocalPlayerData { get; }
    public bool SyncSoil { get; }
}
