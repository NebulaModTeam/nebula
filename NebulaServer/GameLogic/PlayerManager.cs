using LiteNetLib;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NebulaServer.GameLogic
{
    public class PlayerManager
    {
        private readonly Dictionary<NebulaConnection, Player> pendingPlayers;
        private readonly Dictionary<NebulaConnection, Player> syncingPlayers;
        private readonly Dictionary<NebulaConnection, Player> connectedPlayers;

        public PlayerManager()
        {
            pendingPlayers = new Dictionary<NebulaConnection, Player>();
            syncingPlayers = new Dictionary<NebulaConnection, Player>();
            connectedPlayers = new Dictionary<NebulaConnection, Player>();
        }

        public IEnumerable<Player> GetAllPlayers()
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
            foreach (Player player in GetAllPlayers())
            {
                player.SendPacket(packet, deliveryMethod);
            }
        }

        public void SendPacketToOtherPlayers<T>(T packet, Player sender, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            foreach (Player player in GetAllPlayers())
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
            Player newPlayer = new Player(conn);

            if (connectedPlayers.Count == 0)
            {
                newPlayer.IsMasterClient = true;
            }

            pendingPlayers.Add(conn, newPlayer);

            return newPlayer;
        }

        public void PlayerSentHandshake(NebulaConnection connection, HandshakeHello handshake)
        {
            Player player;
            if (!pendingPlayers.TryGetValue(connection, out player))
            {
                connection.Disconnect();
                Console.WriteLine("WARNING: Player tried to handshake without being in the pending list");
                return;
            }

            pendingPlayers.Remove(connection);

            if (handshake.ProtocolVersion != 0) //TODO: Maybe have a shared constants file somewhere for this
            {
                connection.Disconnect();
            }

            var playerList = GetAllPlayers();

            // Add the new player to the list
            if (!player.IsMasterClient)
            {
                syncingPlayers.Add(connection, player);
            }

            player.SendPacket(new HandshakeResponse(player.IsMasterClient, playerList.Select(p => p.Id).ToArray()));

            foreach (Player activePlayer in playerList)
            {
                // Make sure that each player that is currently in the game receive that a new player join so they can create its RemotePlayerCharacter
                activePlayer.SendPacket(new RemotePlayerJoined(player.Id));
            }

            if (player.IsMasterClient)
            {
                connectedPlayers.Add(connection, player);
            }
        }

        public void PlayerDisconnected(NebulaConnection conn)
        {
            if (connectedPlayers.TryGetValue(conn, out Player player))
            {
                SendPacketToOtherPlayers(new PlayerDisconnected(conn.Id), player);
                connectedPlayers.Remove(conn);
            }
        }

        public void PlayerSentSyncComplete(Player player)
        {
            syncingPlayers.Remove(player.connection);
            connectedPlayers.Add(player.connection, player);

            // Signal to all the other users that they can now unpause
            SendPacketToOtherPlayers(new SyncComplete(), player);

            // Send a confirmation to the new player contaning his player id.
            player.SendPacket(new JoinSessionConfirmed(player.Id));
        }

        public void PlayerSentInitialState(Player player, InitialState packet)
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
    }
}
