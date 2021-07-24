using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        public override void ProcessPacket(PingPacket packet, NebulaConnection conn)
        {
            MultiplayerClientSession.Instance.UpdatePingIndicator();
        }
    }
}
