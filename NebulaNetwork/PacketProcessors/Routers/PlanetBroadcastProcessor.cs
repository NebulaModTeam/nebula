using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class PlanetBroadcastProcessor : PacketProcessor<PlanetBroadcastPacket>
    {
        private PlayerManager playerManager;
        public PlanetBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }
        public override void ProcessPacket(PlanetBroadcastPacket packet, NetworkConnection conn)
        {
            if (IsClient) return;

            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                //playerManager.SendRawPacketToPlanet(packet.PacketObject, packet.PlanetId, conn);
                //Forward packet to the host
                MultiplayerHostSession.Instance.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}