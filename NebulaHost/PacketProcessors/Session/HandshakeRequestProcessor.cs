using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaHost.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeRequestProcessor : IPacketProcessor<HandshakeRequest>
    {
        private PlayerManager playerManager;

        public HandshakeRequestProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(HandshakeRequest packet, NebulaConnection conn)
        {
            playerManager.OnPlayerHandshake(conn, packet);
        }
    }
}
