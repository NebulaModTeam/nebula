using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using FactoryManager = NebulaWorld.Factory.FactoryManager;
using LocalPlayer = NebulaWorld.LocalPlayer;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
    {
        private IPlayerManager playerManager;

        public SyncCompleteProcessor()
        {
            playerManager = ((NetworkProvider)Multiplayer.Session.Network).PlayerManager;
        }

        public override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                NebulaPlayer player = playerManager.GetSyncingPlayer(conn);
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
                        player.SendPacket(new NameInputPacket(s.overrideName, s.id, NebulaModAPI.PLANET_NONE, ((LocalPlayer)Multiplayer.Session.LocalPlayer).Id));
                    }

                    foreach (PlanetData p in s.planets)
                    {
                        if (!string.IsNullOrEmpty(p.overrideName))
                        {
                            player.SendPacket(new NameInputPacket(p.overrideName, NebulaModAPI.STAR_NONE, p.id, ((LocalPlayer)Multiplayer.Session.LocalPlayer).Id));
                        }
                    }
                }

                // Since the player is now connected, we can safely spawn his player model
                Multiplayer.Session.World.SpawnRemotePlayerModel(player.Data);

                if (syncingCount == 0)
                {
                    var inGamePlayersDatas = playerManager.GetAllPlayerDataIncludingHost();
                    playerManager.SendPacketToAllPlayers(new SyncComplete(inGamePlayersDatas));
                    Multiplayer.Session.World.OnAllPlayersSyncCompleted();
                }
            }
            else // IsClient
            {
                // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
                foreach (var playerData in packet.AllPlayers)
                {
                    if (playerData.PlayerId != ((LocalPlayer)Multiplayer.Session.LocalPlayer).Id)
                    {
                        Multiplayer.Session.World.SpawnRemotePlayerModel(playerData);
                    }
                }

                Multiplayer.Session.World.OnAllPlayersSyncCompleted();
            }
        }
    }
}
