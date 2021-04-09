using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Processors;
using NebulaModel.Packets.Routers;

namespace NebulaHost.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class StarBroadcastProcessor : IPacketProcessor<StarBroadcastPacket>
    {
        private PlayerManager playerManager;
        public StarBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToStar(packet.PacketObject, packet.StarId, conn);
                MultiplayerHostSession.Instance.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}