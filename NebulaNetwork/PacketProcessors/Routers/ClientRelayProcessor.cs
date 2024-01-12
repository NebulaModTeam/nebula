#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Routers;

[RegisterPacketProcessor]
internal class ClientRelayProcessor : PacketProcessor<ClientRelayPacket>
{
    private readonly IPlayerManager playerManager;

    public ClientRelayProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(ClientRelayPacket packet, NebulaConnection conn)
    {
        // TODO: check if this player null check is required
        var player = playerManager.GetPlayer(conn);
        if (player == null || packet.PacketObject == null)
        {
            return;
        }

        // If the processor is either host or client and the recipient is the processor unwrap and process the packet
        // (This happens in the case that either client -> host or host -> client sends this packet directly,
        // as you do not want manually account for the sender recipient relation when using this packet we have to handle this case as wel)
        if (Multiplayer.Session.LocalPlayer.Data.PlayerId == packet.ClientUserId)
        {
            //Process the packet on the host
            // NOTE: Since PacketProcessor is not part of INetworkProvider this will not work if we ever swap it out with another network provider that does not expose this
            // However StarBroadcastProcessor and PlanetBroadcastProcessor also do this in a similar manner so I think it is fine for now
            ((NetworkProvider)Multiplayer.Session.Network).PacketProcessor
                .EnqueuePacketForProcessing(packet.PacketObject, conn);
            return;
        }

        // If the processor is client and not the recipient, do nothing
        if (IsClient)
        {
            return;
        }

        // If the processor is the host and not the recipient, unwrap and relay packet to recipient client
        var recipient = Multiplayer.Session.Network
            .PlayerManager.GetPlayerById(packet.ClientUserId);
        if (recipient == null)
        {
            Log.Warn($"Could not relay packet because client was not found with clientId: {packet.ClientUserId}");

            // TODO: We might want to communicate back to the sender that the relaying failed
            return;
        }

        recipient.Connection.SendRawPacket(packet.PacketObject);
    }
}
