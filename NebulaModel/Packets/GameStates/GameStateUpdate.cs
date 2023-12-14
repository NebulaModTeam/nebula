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

    public long SentTime { get; }
    public long GameTick { get; }
    public float UnitsPerSecond { get; }
}
