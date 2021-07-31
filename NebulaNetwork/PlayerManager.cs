using Mirror;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using Mirror;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Session;
using NebulaNetwork.PacketProcessors.Players;
using NebulaWorld;
using NebulaWorld.Player;
using NebulaWorld.Statistics;
using System.Collections.Generic;
using System.Threading;
using Config = NebulaModel.Config;

namespace NebulaNetwork
{
    public class PlayerManager
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<NetworkConnection, Player> pendingPlayers = new Dictionary<NetworkConnection, Player>();
            internal readonly Dictionary<NetworkConnection, Player> syncingPlayers = new Dictionary<NetworkConnection, Player>();
            internal readonly Dictionary<NetworkConnection, Player> connectedPlayers = new Dictionary<NetworkConnection, Player>();
            internal readonly Dictionary<string, PlayerData> savedPlayerData = new Dictionary<string, PlayerData>();
            internal readonly Queue<ushort> availablePlayerIds = new Queue<ushort>();
        }

        private readonly ThreadSafe threadSafe = new ThreadSafe();
        private int highestPlayerID = 0;

        public NetPacketProcessor PacketProcessor { get; set; }

        public Locker GetPendingPlayers(out Dictionary<NetworkConnection, Player> pendingPlayers) =>
            threadSafe.pendingPlayers.GetLocked(out pendingPlayers);

        public Locker GetSyncingPlayers(out Dictionary<NetworkConnection, Player> syncingPlayers) =>
            threadSafe.syncingPlayers.GetLocked(out syncingPlayers);

        public Locker GetConnectedPlayers(out Dictionary<NetworkConnection, Player> connectedPlayers) =>
            threadSafe.connectedPlayers.GetLocked(out connectedPlayers);

        public Locker GetSavedPlayerData(out Dictionary<string, PlayerData> savedPlayerData) =>
            threadSafe.savedPlayerData.GetLocked(out savedPlayerData);

        public PlayerData[] GetAllPlayerDataIncludingHost()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                int i = 0;
                var result = new PlayerData[1 + connectedPlayers.Count];
                result[i++] = LocalPlayer.Data;
                foreach (var kvp in connectedPlayers)
                {
                    result[i++] = kvp.Value.Data;
                }

                return result;
            }
        }

        public Player GetPlayer(NetworkConnection conn)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out Player player))
                {
                    return player;
                }
            }

            return null;
        }

        public Player GetSyncingPlayer(NetworkConnection conn)
        {
            using (GetSyncingPlayers(out var syncingPlayers))
            {
                if (syncingPlayers.TryGetValue(conn, out Player player))
                {
                    return player;
                }
            }

            return null;
        }

        public void SendPacketToAllPlayers<T>(T packet) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    Player player = kvp.Value;
                    player.SendPacket(packet);
                }
            }

        }

        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalStarId == GameMain.data.localStar?.id)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalPlanetId == GameMain.data.mainPlayer.planetId)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalPlanetId == planetId)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalStarId == starId)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToStarExcept<T>(T packet, int starId, NetworkConnection exclude) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalStarId == starId && player != GetPlayer(exclude))
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public void SendPacketToOtherPlayers<T>(T packet, Player sender) where T : class, new()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player != sender)
                    {
                        player.SendPacket(packet);
                    }
                }
            }
        }

        public Player PlayerConnected(NetworkConnection conn)
        {
            //Generate new data for the player
            ushort playerId = GetNextAvailablePlayerId();

            Float3 PlayerColor = new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255);
            PlanetData birthPlanet = GameMain.galaxy.PlanetById(GameMain.galaxy.birthPlanetId);
            PlayerData playerData;
            if (LocalPlayer.GS2_GSSettings != null)
            {
                playerData = new PlayerData(playerId, -1, PlayerColor, position: new Double3(birthPlanet.uPosition.x, birthPlanet.uPosition.y, birthPlanet.uPosition.z));
            }
            else
            {
                playerData = new PlayerData(playerId, -1, PlayerColor);
            }

            Player newPlayer = new Player(conn, playerData, PacketProcessor);
            using (GetPendingPlayers(out var pendingPlayers))
            {
                pendingPlayers.Add(conn, newPlayer);
            }

            return newPlayer;
        }

        public void PlayerDisconnected(NetworkConnection conn)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out Player player))
                {
                    SendPacketToOtherPlayers(new PlayerDisconnected(player.Id), player);
                    SimulatedWorld.DestroyRemotePlayerModel(player.Id);
                    connectedPlayers.Remove(conn);
                    using (threadSafe.availablePlayerIds.GetLocked(out var availablePlayerIds))
                    {
                        availablePlayerIds.Enqueue(player.Id);
                    }
                    StatisticsManager.instance.UnRegisterPlayer(player.Id);

                    //Notify players about queued building plans for drones
                    int[] DronePlans = DroneManager.GetPlayerDronePlans(player.Id);
                    if (DronePlans != null && DronePlans.Length > 0 && player.Data.LocalPlanetId > 0)
                    {
                        LocalPlayer.SendPacketToPlanet(new RemoveDroneOrdersPacket(DronePlans), player.Data.LocalPlanetId);
                        //Remove it also from host queue, if host is on the same planet
                        if (GameMain.mainPlayer.planetId == player.Data.LocalPlanetId)
                        {
                            for (int i = 0; i < DronePlans.Length; i++)
                            {
                                GameMain.mainPlayer.mecha.droneLogic.serving.Remove(DronePlans[i]);
                            }
                        }
                    }
                }
                else
                {
                    Log.Warn($"PlayerDisconnected NOT CALLED!");
                }

                // TODO: Should probably also handle playing that disconnect during "pending" or "syncing" steps.
            }
        }

        public ushort GetNextAvailablePlayerId()
        {
            using (threadSafe.availablePlayerIds.GetLocked(out var availablePlayerIds))
            {
                if (availablePlayerIds.Count > 0)
                    return availablePlayerIds.Dequeue();
            }

            return (ushort)Interlocked.Increment(ref highestPlayerID); // this is truncated to ushort.MaxValue
        }

        public void UpdateMechaData(MechaData mechaData, NetworkConnection conn)
        {
            if (mechaData == null)
            {
                return;
            }
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out Player player))
                {
                    //Find correct player for data to update
                    player.Data.Mecha = mechaData;
                }
            }
        }

        public void SendTechRefundPackagesToClients(int techId)
        {
            //send players their contributions back
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    Player curPlayer = kvp.Value;
                    long techProgress = curPlayer.ReleaseResearchProgress();

                    if (techProgress > 0)
                    {
                        Log.Info($"Sending Recoverrequest for player {curPlayer.Id}: refunding for techId {techId} - raw progress: {curPlayer.TechProgressContributed}");
                        GameHistoryTechRefundPacket refundPacket = new GameHistoryTechRefundPacket(techId, techProgress);
                        curPlayer.SendPacket(refundPacket);
                    }
                }
            }
        }
    }
}

