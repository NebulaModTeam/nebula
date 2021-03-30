using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Logistics;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class StationUIBroadcaster: IPacketProcessor<StationUI>
    {
        private PlayerManager playerManager;
        public StationUIBroadcaster()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }
        public void ProcessPacket(StationUI packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            if (player != null)
            {
                playerManager.SendPacketToOtherPlayers(packet, player);
            }
        }
    }
}
