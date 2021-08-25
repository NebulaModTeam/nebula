using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class PlanetBroadcastProcessor : PacketProcessor<PlanetBroadcastPacket>
    {
        private IPlayerManager playerManager;
        public PlanetBroadcastProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(PlanetBroadcastPacket packet, NebulaConnection conn)
        {
            if (IsClient) return;

            NebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToPlanet(packet.PacketObject, packet.PlanetId, conn);
                //Forward packet to the host
                Multiplayer.Session.Network.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}