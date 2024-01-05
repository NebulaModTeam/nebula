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
        //Forward packet to other users if we're the host
        if (IsHost)
            Multiplayer.Session.Server.SendIfCondition(packet, p =>
                p.Data.LocalPlanetId == packet.PlanetId &&
                p.Connection.Equals(conn)
            );

        //Forward packet data to be processed
        Multiplayer.Session.Network.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
    }
}
