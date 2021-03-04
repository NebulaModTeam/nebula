using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class JoinSessionConfirmedProcessor : IPacketProcessor<JoinSessionConfirmed>
    {
        private PlayerManager playerManager;

        public JoinSessionConfirmedProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(JoinSessionConfirmed packet, NebulaConnection conn)
        {
            playerManager?.SetLocalPlayer(packet.LocalPlayerId);
        }
    }
}
