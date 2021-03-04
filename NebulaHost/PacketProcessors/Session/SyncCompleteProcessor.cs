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
            playerManager.SyncingPlayers.Remove(player.connection);
            playerManager.ConnectedPlayers.Add(player.connection, player);

            if (playerManager.SyncingPlayers.Count == 0)
            {
                playerManager.SendPacketToOtherPlayers(new SyncComplete(), player);
            }

            // Send a confirmation to the new player containing his player id.
            player.SendPacket(new JoinSessionConfirmed(player.Id));
        }
    }
}
