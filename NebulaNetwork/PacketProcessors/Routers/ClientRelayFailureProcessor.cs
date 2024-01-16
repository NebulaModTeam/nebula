#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Routers;

[RegisterPacketProcessor]
internal class ClientRelayFailureProcessor : PacketProcessor<ClientRelayPacket>
{
    private readonly IPlayerManager playerManager;

    public ClientRelayFailureProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(ClientRelayPacket packet, NebulaConnection conn)
    {
        // This if the processor is the host do nothing (this should never be recieved by the host as it is the relayer)
        if (IsHost)
        {
            return;
        }

        // Else process the wrapped packet
        //((NetworkProvider)Multiplayer.Session.Network).PacketProcessor
        //    .EnqueuePacketForProcessing(packet.PacketObject, conn);

    }
}
