using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    class StarBroadcastProcessor : PacketProcessor<StarBroadcastPacket>
    {
        private IPlayerManager playerManager;
        public StarBroadcastProcessor()
        {
            playerManager = Multiplayer.Session.NetProvider.PlayerManager;
        }
        public override void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
        {
            if (IsClient) return;

            NebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToStar(packet.PacketObject, packet.StarId, conn);
                Multiplayer.Session.NetProvider.PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}