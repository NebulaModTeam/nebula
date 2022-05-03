using NebulaAPI;
using System;

namespace NebulaModel.Packets.GameStates
{
    [HidePacketInDebugLogs]
    public class GameStateRequest
    {
        public long SentTimestamp { get; set; }

        public GameStateRequest()
        {
            SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
