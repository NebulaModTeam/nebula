using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipItemsBroadcastProcessor: IPacketProcessor<ILSShipItems>
    {
        private PlayerManager playerManager;
        public ILSShipItemsBroadcastProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(ILSShipItems packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
