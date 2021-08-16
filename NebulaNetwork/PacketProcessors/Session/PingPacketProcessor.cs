using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;

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
                MultiplayerClientSession.Instance.UpdatePingIndicator();
            }
        }
    }
}
