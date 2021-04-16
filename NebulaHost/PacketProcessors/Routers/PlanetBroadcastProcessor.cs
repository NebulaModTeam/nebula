using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Routers;

namespace NebulaHost.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class PlanetBroadcastProcessor : IPacketProcessor<PlanetBroadcastPacket>
    {
        private PlayerManager playerManager;
        public PlanetBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(PlanetBroadcastPacket packet, NebulaConnection conn)
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