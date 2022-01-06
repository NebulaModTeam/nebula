using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    internal class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        private int averageRTT;

        public override void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                conn.SendPacket(packet);
            }
            else
            {
                int rtt = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.SentTimestamp);
                averageRTT = (int)(averageRTT * 0.7 + rtt * 0.3);
                Multiplayer.Session.World.UpdatePingIndicator($"Ping: {averageRTT}ms");
            }
        }
    }
}
