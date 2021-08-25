using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        public override void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                conn.SendPacket(new PingPacket());
            }
            else
            {
                int rtt = (int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - packet.SentTimestamp);
                Multiplayer.Session.World.UpdatePingIndicator($"Ping: {rtt}ms");
            }
        }
    }
}
