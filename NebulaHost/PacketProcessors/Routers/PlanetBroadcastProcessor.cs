using NebulaModel.Attributes;
using NebulaModel.Networking;
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
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public override void ProcessPacket(PlanetBroadcastPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToPlanet(packet.PacketObject, packet.PlanetId, conn);
                //Forward packet to the host
                MultiplayerHostSession.Instance.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}