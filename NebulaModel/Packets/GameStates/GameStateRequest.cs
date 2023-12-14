#region

using System;
using NebulaAPI;

#endregion

namespace NebulaModel.Packets.GameStates;

[HidePacketInDebugLogs]
public class GameStateRequest
{
    public GameStateRequest()
    {
        SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public long SentTimestamp { get; set; }
}
