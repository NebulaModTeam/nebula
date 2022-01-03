using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;
using System.Collections.Generic;

/*
 * This handler is only here to handle older clients and tell them to upgrade to a newer nebula version. (this packet was replaced by the lobby packets)
 */
namespace NebulaNetwork.PacketProcessors.Session
{
    [RegisterPacketProcessor]
    public class HandshakeRequestProcessor: PacketProcessor<HandshakeRequest>
    {
        private readonly IPlayerManager playerManager;

        public HandshakeRequestProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(HandshakeRequest packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player;
            using (playerManager.GetPendingPlayers(out Dictionary<INebulaConnection, INebulaPlayer> pendingPlayers))
            {
                if (!pendingPlayers.TryGetValue(conn, out player))
                {
                    conn.Disconnect(DisconnectionReason.InvalidData);
                    Log.Warn("WARNING: Player tried to handshake without being in the pending list. And he uses an outdated nebula version.");
                    return;
                }

                pendingPlayers.Remove(conn);
            }

            conn.Disconnect(DisconnectionReason.ModVersionMismatch, $"Nebula;0.7.7 or earlier;0.7.8 or greater");
        }
    }
}
