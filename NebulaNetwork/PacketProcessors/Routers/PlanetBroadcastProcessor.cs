#region

using NebulaAPI.GameState;
using NebulaAPI.Packets;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Routers;

[RegisterPacketProcessor]
internal class PlanetBroadcastProcessor : PacketProcessor<PlanetBroadcastPacket>
{
    private readonly IPlayerManager playerManager;

    public PlanetBroadcastProcessor()
    {
        playerManager = Multiplayer.Session.Network.PlayerManager;
    }

    protected override void ProcessPacket(PlanetBroadcastPacket packet, NebulaConnection conn)
    {
        if (IsClient)
        {
            return;
        }

        var player = playerManager.GetPlayer(conn);
        if (player == null)
        {
            return;
        }
        //Forward packet to other users
        playerManager.SendRawPacketToPlanet(packet.PacketObject, packet.PlanetId, conn);
        //Forward packet to the host
        ((INetworkProvider)Multiplayer.Session.Network).PacketProcessor
            .EnqueuePacketForProcessing(packet.PacketObject, conn);
    }
}
