using NebulaAPI;

namespace NebulaModel.Packets.Session
{
    [HidePacketInDebugLogs]
    public class PingPacket
    {
        public long SentTimestamp { get; set; }

        public PingPacket()
        {
            SentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
