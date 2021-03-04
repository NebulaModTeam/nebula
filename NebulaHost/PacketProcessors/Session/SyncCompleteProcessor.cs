using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;

namespace NebulaHost.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : IPacketProcessor<SyncComplete>
    {
        private PlayerManager playerManager;

        public SyncCompleteProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            Player player = playerManager.GetSyncingPlayer(conn);
            playerManager.OnPlayerSyncComplete(player);
        }
    }
}
