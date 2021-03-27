using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Linq;

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
            if (player == null)
            {
                Log.Warn("Received a SyncComplete packet, but no player is joining.");
                return;
            }

            playerManager.SyncingPlayers.Remove(player.Connection);
            playerManager.ConnectedPlayers.Add(player.Connection, player);

            // Since the player is now connected, we can safely spawn his player model
            SimulatedWorld.SpawnRemotePlayerModel(player.Data);

            if (playerManager.SyncingPlayers.Count == 0)
            {
                var inGamePlayersDatas = playerManager.GetAllPlayerDataIncludingHost();
                playerManager.SendPacketToAllPlayers(new SyncComplete(inGamePlayersDatas.ToArray()));
                SimulatedWorld.OnAllPlayersSyncCompleted();
            }
        }
    }
}
