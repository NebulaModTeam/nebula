using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
    {
        private PlayerManager playerManager;

        public SyncCompleteProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            if (IsHost)
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
                        player.SendPacket(new NameInputPacket(s.overrideName, s.id, FactoryManager.PLANET_NONE, LocalPlayer.PlayerId));
                    }

                    foreach (PlanetData p in s.planets)
                    {
                        if (!string.IsNullOrEmpty(p.overrideName))
                        {
                            player.SendPacket(new NameInputPacket(p.overrideName, FactoryManager.STAR_NONE, p.id, LocalPlayer.PlayerId));
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
            else // IsClient
            {
                // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
                foreach (var playerData in packet.AllPlayers)
                {
                    if (playerData.PlayerId != LocalPlayer.PlayerId)
                    {
                        SimulatedWorld.SpawnRemotePlayerModel(playerData);
                    }
                }

                SimulatedWorld.OnAllPlayersSyncCompleted();
            }
        }
    }
}
