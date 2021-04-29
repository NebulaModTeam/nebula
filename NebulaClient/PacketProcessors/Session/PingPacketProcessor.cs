using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class PingPacketProcessor : IPacketProcessor<PingPacket>
    {
        public void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            MultiplayerClientSession.Instance.UpdatePingIndicator();
        }
    }
}
