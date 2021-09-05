using NebulaAPI;
using NebulaModel;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Routers;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Routers
{
    [RegisterPacketProcessor]
    internal class StarBroadcastProcessor : PacketProcessor<StarBroadcastPacket>
    {
        private readonly IPlayerManager playerManager;
        public StarBroadcastProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }
        public override void ProcessPacket(StarBroadcastPacket packet, NebulaConnection conn)
        {
            if (IsClient)
            {
                return;
            }

            INebulaPlayer player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                //Forward packet to other users
                playerManager.SendRawPacketToStar(packet.PacketObject, packet.StarId, conn);
                ((NetworkProvider)Multiplayer.Session.Network).PacketProcessor.EnqueuePacketForProcessing(packet.PacketObject, conn);
            }
        }
    }
}