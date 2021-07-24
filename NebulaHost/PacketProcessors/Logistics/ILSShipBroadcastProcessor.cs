using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipBroadcastProcessor: PacketProcessor<ILSShipData>
    {
        private PlayerManager playerManager;
        public ILSShipBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public override void ProcessPacket(ILSShipData packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
