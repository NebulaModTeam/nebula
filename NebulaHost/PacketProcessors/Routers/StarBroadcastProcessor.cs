using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class StarBroadcastProcessor : PacketProcessor<StarBroadcastPacket>
    {
        private PlayerManager playerManager;
        public StarBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public override void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
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