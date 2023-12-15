#region

using System;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.GameStates;

[HidePacketInDebugLogs]
public class GameStateRequest
{
    public long SentTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
