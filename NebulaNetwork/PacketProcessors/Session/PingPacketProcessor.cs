using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaModel.Networking;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    class PingPacketProcessor : PacketProcessor<PingPacket>
    {
        public override void ProcessPacket(PingPacket packet, NetworkConnection conn)
        {
            if (IsHost)
            {
                conn.SendPacket(new PingPacket());
            }
            else
            {
                MultiplayerClientSession.Instance.UpdatePingIndicator();
            }
        }
    }
}
