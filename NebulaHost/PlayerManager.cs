using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NebulaHost
{
    public class PlayerManager
    {
        private readonly ThreadSafeDictionary<NebulaConnection, Player> pendingPlayers;
        private readonly ThreadSafeDictionary<NebulaConnection, Player> syncingPlayers;
        private readonly ThreadSafeDictionary<NebulaConnection, Player> connectedPlayers;
        private readonly ThreadSafeDictionary<string, PlayerData> savedPlayerData;
        private readonly ThreadSafeQueue<ushort> availablePlayerIds;
        private ushort highestPlayerID = 0;

        public PlayerManager()
        {
            pendingPlayers = new ThreadSafeDictionary<NebulaConnection, Player>();
            syncingPlayers = new ThreadSafeDictionary<NebulaConnection, Player>();
            connectedPlayers = new ThreadSafeDictionary<NebulaConnection, Player>();
            savedPlayerData = new ThreadSafeDictionary<string, PlayerData>();
            availablePlayerIds = new ThreadSafeQueue<ushort>();
        }

        public ThreadSafeDictionary<NebulaConnection, Player> PendingPlayers => pendingPlayers;
        public ThreadSafeDictionary<NebulaConnection, Player> SyncingPlayers => syncingPlayers;
        public ThreadSafeDictionary<NebulaConnection, Player> ConnectedPlayers => connectedPlayers;
        public ThreadSafeDictionary<string, PlayerData> SavedPlayerData => savedPlayerData;

        public IEnumerable<PlayerData> GetAllPlayerDataIncludingHost()
        {
            return new PlayerData[] { LocalPlayer.Data }.Concat(GetConnectedPlayers().Select(p => p.Data));
        }

        public IEnumerable<Player> GetConnectedPlayers()
        {
            return connectedPlayers.Values;
        }

        public Player GetPlayer(NebulaConnection conn)
        {
            if (connectedPlayers.TryGetValue(conn, out Player player))
            {
                return player;
            }
            return null;
        }

        public Player GetSyncingPlayer(NebulaConnection conn)
        {
            if (syncingPlayers.TryGetValue(conn, out Player player))
            {
                return player;
            }

            return null;
        }

        public void SendPacketToAllPlayers<T>(T packet) where T : class, new()
        {
            foreach (Player player in GetConnectedPlayers())
            {
                player.SendPacket(packet);
            }
        }

        public void SendPacketToOtherPlayers<T>(T packet, Player sender) where T : class, new()
        {
            foreach (Player player in GetConnectedPlayers())
            {
                if (player != sender)
                {
                    player.SendPacket(packet);
                }
            }
        }

        public Player PlayerConnected(NebulaConnection conn)
        {
            //Generate new data for the player
            ushort playerId = GetNextAvailablePlayerId();
            Float3 randomColor = new Float3(Random.value, Random.value, Random.value);
            PlayerData playerData = new PlayerData(playerId, -1, randomColor);

            Player newPlayer = new Player(conn, playerData);
            pendingPlayers.Add(conn, newPlayer);

            return newPlayer;
        }

        public void PlayerDisconnected(NebulaConnection conn)
        {
            if (connectedPlayers.TryGetValue(conn, out Player player))
            {
                SendPacketToOtherPlayers(new PlayerDisconnected(player.Id), player);
                SimulatedWorld.DestroyRemotePlayerModel(player.Id);
                connectedPlayers.Remove(conn);
                availablePlayerIds.Enqueue(player.Id);
            }
            else
            {
                Log.Warn($"PlayerDisconnected NOT CALLED!");
            }

            // TODO: Should probably also handle playing that disconnect during "pending" or "syncing" steps.
        }

        public ushort GetNextAvailablePlayerId()
        {
            if (availablePlayerIds.Count > 0)
                return availablePlayerIds.Dequeue();
            else
                return ++highestPlayerID;
        }

        public void UpdateMechaData(MechaData mechaData, NebulaConnection conn)
        {
            if (mechaData == null)
            {
                return;
            }
            if (connectedPlayers.TryGetValue(conn, out Player player))
            {
                //Find correct player for data to update
                player.Data.Mecha = mechaData;
            }
        }
    }
}
