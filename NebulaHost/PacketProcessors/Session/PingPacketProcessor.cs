using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaHost.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class PingPacketProcessor : IPacketProcessor<PingPacket>
    {
        public void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            //Reply to the client
            conn.SendPacket(new PingPacket());
        }
    }
}
