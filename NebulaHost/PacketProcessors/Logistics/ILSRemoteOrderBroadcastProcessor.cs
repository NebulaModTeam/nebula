using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSRemoteOrderBroadcastProcessor : PacketProcessor<ILSRemoteOrderData>
    {
        private PlayerManager playerManager;
        public ILSRemoteOrderBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public override void ProcessPacket(ILSRemoteOrderData packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
