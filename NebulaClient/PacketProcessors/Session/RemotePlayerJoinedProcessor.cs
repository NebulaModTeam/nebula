using NebulaClient.GameLogic;
using NebulaClient.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaClient.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class RemotePlayerJoinedProcessor : IPacketProcessor<RemotePlayerJoined>
    {
        private PlayerManager playerManager;

        public RemotePlayerJoinedProcessor()
        {
            playerManager = MultiplayerClientSession.Instance.PlayerManager;
        }

        public void ProcessPacket(RemotePlayerJoined packet, NebulaConnection conn)
        {
            playerManager.AddRemotePlayer(packet.PlayerId);
            GameMain.Pause();
        }
    }
}
