using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerMechaDataProcessor : IPacketProcessor<PlayerMechaData>
    {
        private PlayerManager playerManager;

        public PlayerMechaDataProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(PlayerMechaData packet, NebulaConnection conn)
        {
            playerManager.UpdateMechaData(packet.Data, conn);
        }
    }
}