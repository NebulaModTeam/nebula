using NebulaModel.Attributes;
using Mirror;
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
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }
        public override void ProcessPacket(StarBroadcastPacket packet, NetworkConnection conn)
        {
            if (IsClient) return;

            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                //playerManager.SendRawPacketToStar(packet.PacketObject, packet.StarId, conn);
                MultiplayerHostSession.Instance.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}