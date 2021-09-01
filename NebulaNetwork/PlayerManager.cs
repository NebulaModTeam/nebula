using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.Session;
using NebulaNetwork.PacketProcessors.Players;
using NebulaWorld;
using System.Collections.Generic;
using System.Threading;
using Config = NebulaModel.Config;

namespace NebulaNetwork
{
    public class PlayerManager : IPlayerManager
    {
        sealed class ThreadSafe
        {
            internal readonly Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers = new Dictionary<INebulaConnection, INebulaPlayer>();
            internal readonly Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers = new Dictionary<INebulaConnection, INebulaPlayer>();
            internal readonly Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers = new Dictionary<INebulaConnection, INebulaPlayer>();
            internal readonly Dictionary<string, IPlayerData> savedPlayerData = new Dictionary<string, IPlayerData>();
            internal readonly Queue<ushort> availablePlayerIds = new Queue<ushort>();
        }

        private readonly ThreadSafe threadSafe = new ThreadSafe();
        private int highestPlayerID = 0;

        public Locker GetPendingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers) =>
            threadSafe.pendingPlayers.GetLocked(out pendingPlayers);

        public Locker GetSyncingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> syncingPlayers) =>
            threadSafe.syncingPlayers.GetLocked(out syncingPlayers);

        public Locker GetConnectedPlayers(out Dictionary<INebulaConnection, INebulaPlayer> connectedPlayers) =>
            threadSafe.connectedPlayers.GetLocked(out connectedPlayers);

        public Locker GetSavedPlayerData(out Dictionary<string, IPlayerData> savedPlayerData) =>
            threadSafe.savedPlayerData.GetLocked(out savedPlayerData);

        public IPlayerData[] GetAllPlayerDataIncludingHost()
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                int i = 0;
                var result = new IPlayerData[1 + connectedPlayers.Count];
                result[i++] = Multiplayer.Session.LocalPlayer.Data;
                foreach (var kvp in connectedPlayers)
                {
                    result[i++] = kvp.Value.Data;
                }

                return result;
            }
        }

        public INebulaPlayer GetPlayer(INebulaConnection conn)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out INebulaPlayer player))
                {
                    return player;
                }
            }

            return null;
        }

        public INebulaPlayer GetSyncingPlayer(INebulaConnection conn)
        {
            using (GetSyncingPlayers(out var syncingPlayers))
            {
                if (syncingPlayers.TryGetValue(conn, out INebulaPlayer player))
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
                    INebulaPlayer player = kvp.Value;
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

        public void SendPacketToStarExcept<T>(T packet, int starId, INebulaConnection exclude) where T : class, new()
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

        public void SendRawPacketToStar(byte[] rawPacket, int starId, INebulaConnection sender)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalStarId == starId && player.Connection != sender)
                    {
                        ((NebulaPlayer)player).SendRawPacket(rawPacket);
                    }
                }
            }
        }

        public void SendRawPacketToPlanet(byte[] rawPacket, int planetId, INebulaConnection sender)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                foreach (var kvp in connectedPlayers)
                {
                    var player = kvp.Value;
                    if (player.Data.LocalPlanetId == planetId && player.Connection != sender)
                    {
                        ((NebulaPlayer)player).SendRawPacket(rawPacket);
                    }
                }
            }
        }

        public void SendPacketToOtherPlayers<T>(T packet, INebulaPlayer sender) where T : class, new()
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

        public INebulaPlayer PlayerConnected(INebulaConnection conn)
        {
            // Generate new data for the player
            ushort playerId = GetNextAvailablePlayerId();

            Float3 playerColor = new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255);
            PlanetData birthPlanet = GameMain.galaxy.PlanetById(GameMain.galaxy.birthPlanetId);
            PlayerData playerData = new PlayerData(playerId, -1, playerColor, position: new Double3(birthPlanet.uPosition.x, birthPlanet.uPosition.y, birthPlanet.uPosition.z));

            INebulaPlayer newPlayer = new NebulaPlayer((NebulaConnection)conn, playerData);
            using (GetPendingPlayers(out var pendingPlayers))
            {
                pendingPlayers.Add(conn, newPlayer);
            }

            return newPlayer;
        }

        public void PlayerDisconnected(INebulaConnection conn)
        {
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out INebulaPlayer player))
                {
                    SendPacketToOtherPlayers(new PlayerDisconnected(player.Id), player);
                    Multiplayer.Session.World.DestroyRemotePlayerModel(player.Id);
                    connectedPlayers.Remove(conn);
                    using (threadSafe.availablePlayerIds.GetLocked(out var availablePlayerIds))
                    {
                        availablePlayerIds.Enqueue(player.Id);
                    }
                    Multiplayer.Session.Statistics.UnRegisterPlayer(player.Id);

                    //Notify players about queued building plans for drones
                    int[] DronePlans = Multiplayer.Session.Drones.GetPlayerDronePlans(player.Id);
                    if (DronePlans != null && DronePlans.Length > 0 && player.Data.LocalPlanetId > 0)
                    {
                        Multiplayer.Session.Network.SendPacketToPlanet(new RemoveDroneOrdersPacket(DronePlans), player.Data.LocalPlanetId);
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

        public void UpdateMechaData(IMechaData mechaData, INebulaConnection conn)
        {
            if (mechaData == null)
            {
                return;
            }
            using (GetConnectedPlayers(out var connectedPlayers))
            {
                if (connectedPlayers.TryGetValue(conn, out INebulaPlayer player))
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
                    INebulaPlayer curPlayer = kvp.Value;
                    long techProgress = ((NebulaPlayer)curPlayer).ReleaseResearchProgress();

                    if (techProgress > 0)
                    {
                        Log.Info($"Sending Recover request for player {curPlayer.Id}: refunding for techId {techId} - raw progress: {curPlayer.TechProgressContributed}");
                        GameHistoryTechRefundPacket refundPacket = new GameHistoryTechRefundPacket(techId, techProgress);
                        curPlayer.SendPacket(refundPacket);
                    }
                }
            }
        }
    }
}

