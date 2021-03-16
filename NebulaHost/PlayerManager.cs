using NebulaModel.DataStructures;
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
        private readonly Dictionary<NebulaConnection, Player> pendingPlayers;
        private readonly Dictionary<NebulaConnection, Player> syncingPlayers;
        private readonly Dictionary<NebulaConnection, Player> connectedPlayers;

        private readonly Queue<ushort> availablePlayerIds;
        private ushort highestPlayerID = 0;

        public PlayerManager()
        {
            pendingPlayers = new Dictionary<NebulaConnection, Player>();
            syncingPlayers = new Dictionary<NebulaConnection, Player>();
            connectedPlayers = new Dictionary<NebulaConnection, Player>();
            availablePlayerIds = new Queue<ushort>();
        }

        public Dictionary<NebulaConnection, Player> PendingPlayers => pendingPlayers;
        public Dictionary<NebulaConnection, Player> SyncingPlayers => syncingPlayers;
        public Dictionary<NebulaConnection, Player> ConnectedPlayers => connectedPlayers;

        public IEnumerable<PlayerData> GetAllPlayerIdsIncludingHost()
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
            // TODO: Load old player state if we have one. We generate a random one for now.
            ushort playerId = GetNextAvailablePlayerId();
            Float3 randomColor = new Float3(Random.value, Random.value, Random.value);
            PlayerData playerData = new PlayerData(playerId, randomColor);

            Player newPlayer = new Player(conn, playerData);
            pendingPlayers.Add(conn, newPlayer);

            return newPlayer;
        }

        public void PlayerDisconnected(NebulaConnection conn)
        {
            if (connectedPlayers.TryGetValue(conn, out Player player))
            {
                SendPacketToOtherPlayers(new PlayerDisconnected(player.Id), player);
                connectedPlayers.Remove(conn);
                availablePlayerIds.Enqueue(player.Id);
                SimulatedWorld.DestroyRemotePlayerModel(player.Id);
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
    }
}
