using LiteNetLib;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<ushort> GetAllPlayerIdsIncludingHost()
        {
            return new ushort[] { LocalPlayer.PlayerId }.Concat(GetConnectedPlayers().Select(p => p.Id));
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

        public void SendPacketToAllPlayers<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            foreach (Player player in GetConnectedPlayers())
            {
                player.SendPacket(packet, deliveryMethod);
            }
        }

        public void SendPacketToOtherPlayers<T>(T packet, Player sender, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            foreach (Player player in GetConnectedPlayers())
            {
                if (player != sender)
                {
                    player.SendPacket(packet, deliveryMethod);
                }
            }
        }

        public Player PlayerConnected(NebulaConnection conn)
        {
            // TODO: Load old player state if we have one. Perhaps some sort of client-generated UUID, or a steam ID?
            Player newPlayer = new Player(conn, GetNextAvailablePlayerId());
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

        /*
        public void  PlayerSentInitialState(Player player, InitialState packet)
        {
            if (!player.IsMasterClient)
            {
                // Someone is doing something nefarious here
                return;
            }

            foreach (var syncingPlayer in syncingPlayers.Values)
            {
                syncingPlayer.SendPacket(packet);
            }
        }
        */
    }
}
