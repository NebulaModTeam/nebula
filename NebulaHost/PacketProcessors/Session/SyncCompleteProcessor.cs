using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaWorld;

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

            // Should these be locked together?

            int syncingCount;
            using (playerManager.GetSyncingPlayers(out var syncingPlayers))
            {
                bool removed = syncingPlayers.Remove(player.Connection);
                syncingCount = syncingPlayers.Count;
            }

            using (playerManager.GetConnectedPlayers(out var connectedPlayers))
            {
                connectedPlayers.Add(player.Connection, player);
            }

            // Load overriden Planet and Star names
            foreach (StarData s in GameMain.galaxy.stars)
            {
                if (!string.IsNullOrEmpty(s.overrideName))
                {
                    player.SendPacket(new NameInputPacket(s.overrideName, s.id, -1, LocalPlayer.PlayerId));
                }

                foreach (PlanetData p in s.planets)
                {
                    if (!string.IsNullOrEmpty(p.overrideName))
                    {
                        player.SendPacket(new NameInputPacket(p.overrideName, -1, p.id, LocalPlayer.PlayerId));
                    }
                }
            }

            // Since the player is now connected, we can safely spawn his player model
            SimulatedWorld.SpawnRemotePlayerModel(player.Data);

            if (syncingCount == 0)
            {
                var inGamePlayersDatas = playerManager.GetAllPlayerDataIncludingHost();
                playerManager.SendPacketToAllPlayers(new SyncComplete(inGamePlayersDatas));
                SimulatedWorld.OnAllPlayersSyncCompleted();
            }
        }
    }
}
