using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaModel.Packets.Universe;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class SyncCompleteProcessor : PacketProcessor<SyncComplete>
    {
        private readonly IPlayerManager playerManager;

        public SyncCompleteProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(SyncComplete packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                INebulaPlayer player = playerManager.GetSyncingPlayer(conn);
                if (player == null)
                {
                    Log.Warn("Received a SyncComplete packet, but no player is joining.");
                    return;
                }

                // Should these be locked together?

                int syncingCount;
                using (playerManager.GetSyncingPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers))
                {
                    bool removed = syncingPlayers.Remove(player.Connection);
                    syncingCount = syncingPlayers.Count;
                }

                using (playerManager.GetConnectedPlayers(out System.Collections.Generic.Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers))
                {
                    connectedPlayers.Add(player.Connection, player);
                }

                // Load overriden Planet and Star names
                foreach (StarData s in GameMain.galaxy.stars)
                {
                    if (!string.IsNullOrEmpty(s.overrideName))
                    {
                        player.SendPacket(new NameInputPacket(s.overrideName, s.id, NebulaModAPI.PLANET_NONE, Multiplayer.Session.LocalPlayer.Id));
                    }

                    foreach (PlanetData p in s.planets)
                    {
                        if (!string.IsNullOrEmpty(p.overrideName))
                        {
                            player.SendPacket(new NameInputPacket(p.overrideName, NebulaModAPI.STAR_NONE, p.id, Multiplayer.Session.LocalPlayer.Id));
                        }
                    }
                }

                // Since the player is now connected, we can safely spawn his player model
                Multiplayer.Session.World.SpawnRemotePlayerModel(player.Data);

                if (syncingCount == 0)
                {
                    IPlayerData[] inGamePlayersDatas = playerManager.GetAllPlayerDataIncludingHost();
                    playerManager.SendPacketToAllPlayers(new SyncComplete(inGamePlayersDatas));
                    Multiplayer.Session.World.OnAllPlayersSyncCompleted();
                }
            }
            else // IsClient
            {
                // Everyone is now connected, we can safely spawn the player model of all the other players that are currently connected
                foreach (NebulaModel.DataStructures.PlayerData playerData in packet.AllPlayers)
                {
                    if (playerData.PlayerId != Multiplayer.Session.LocalPlayer.Id)
                    {
                        Multiplayer.Session.World.SpawnRemotePlayerModel(playerData);
                    }
                }

                Multiplayer.Session.World.OnAllPlayersSyncCompleted();
            }
        }
    }
}
