using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Routers;
using NebulaWorld;

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

                //Host probably does not need to know about flying drones of other players if he is not on the same planet
                if (LocalPlayer.Data.LocalPlanetId == packet.PlanetId)
                {
                    MultiplayerHostSession.Instance.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
                }
            }
        }
    }
}