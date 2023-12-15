#region

using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.GameStates;

[HidePacketInDebugLogs]
public class GameStateUpdate
{
    public GameStateUpdate() { }

    public GameStateUpdate(long sentTime, long gameTick, float unitsPerSecond)
    {
        SentTime = sentTime;
        GameTick = gameTick;
        UnitsPerSecond = unitsPerSecond;
    }

    public long SentTime { get; set; }
    public long GameTick { get; set; }
    public float UnitsPerSecond { get; set; }
}
