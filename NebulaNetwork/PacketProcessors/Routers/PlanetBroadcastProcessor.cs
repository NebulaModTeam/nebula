using NebulaAPI;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    internal class PlanetBroadcastProcessor : PacketProcessor<PlanetBroadcastPacket>
    {
        private readonly IPlayerManager playerManager;
        public PlanetBroadcastProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(PlanetBroadcastPacket packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToPlanet(packet.PacketObject, packet.PlanetId, conn);
                //Forward packet to the host
                ((NetworkProvider)Multiplayer.Session.Network).PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}