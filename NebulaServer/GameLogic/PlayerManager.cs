using LiteNetLib;
using NebulaModel.Networking;
using NebulaModel.Packets;
using System.Collections.Generic;

namespace NebulaServer.GameLogic
{
    public class PlayerManager
    {
        private readonly Dictionary<NebulaConnection, Player> connectedPlayers;

        public PlayerManager()
        {
            connectedPlayers = new Dictionary<NebulaConnection, Player>();
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return connectedPlayers.Values;
        }

        public Player GetPlayer(NebulaConnection conn)
        {
            Player player;
            if (connectedPlayers.TryGetValue(conn, out player))
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
            // TODO: Load old player state if we have one. Lookup using conn.Endpoint maybe ??
            Player newPlayer = new Player(conn);

            foreach (Player player in GetAllPlayers())
            {
                // Make sure that each player that is currently in the game receive that a new player join so they can create its RemotePlayerCharacter
                player.SendPacket(new RemotePlayerJoined(newPlayer.Id));
                
                // TODO: This could probably be done in the initial game state packet instead
                // For now we will fake it, by sending a PlayerJoined packet to the new player for each player already joined.
                // This will make sure that the new player creates a RemotePlayerCharacter for each players in the session.
                newPlayer.SendPacket(new RemotePlayerJoined(player.Id));
            }

            // Add the new player to the list
            connectedPlayers.Add(conn, newPlayer);

            // Send a confirmation to the new player contaning his player id.
            newPlayer.SendPacket(new JoinSessionConfirmed(newPlayer.Id));

            return newPlayer;
        }

        public void PlayerDisconnected(NebulaConnection conn)
        {
            Player player;
            if (connectedPlayers.TryGetValue(conn, out player))
            {
                SendPacketToOtherPlayers(new PlayerDisconnected(conn.Id), player);
                connectedPlayers.Remove(conn);
            }
        }
    }
}
