#region

using System;
using NebulaAPI.Packets;

#endregion

namespace NebulaModel.Packets.GameStates;

[HidePacketInDebugLogs]
public class GameStateRequest
{
    public long SentTimestamp { get; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}
