#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Session;
using NebulaWorld;

#endregion

/*
 * This handler is only here to handle older clients and tell them to upgrade to a newer nebula version. (this packet was replaced by the lobby packets)
 */
namespace NebulaNetwork.PacketProcessors.Session;

[RegisterPacketProcessor]
public class HandshakeRequestProcessor : PacketProcessor<HandshakeRequest>
{
    private readonly IPlayerManager playerManager = Multiplayer.Session.Network.PlayerManager;

    protected override void ProcessPacket(HandshakeRequest packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        using (playerManager.GetPendingPlayers(out var pendingPlayers))
        {
            if (!pendingPlayers.TryGetValue(conn, out _))
            {
                conn.Disconnect(DisconnectionReason.InvalidData);
                Log.Warn(
                    "WARNING: Player tried to handshake without being in the pending list. And he uses an outdated nebula version.");
                return;
            }

            pendingPlayers.Remove(conn);
        }

        conn.Disconnect(DisconnectionReason.ModVersionMismatch, "Nebula;0.7.7 or earlier;0.7.8 or greater");
    }
}
