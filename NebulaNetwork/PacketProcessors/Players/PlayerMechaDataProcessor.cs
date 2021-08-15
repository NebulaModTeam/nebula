using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerMechaDataProcessor : PacketProcessor<PlayerMechaData>
    {
        private PlayerManager playerManager;

        public PlayerMechaDataProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(PlayerMechaData packet, NebulaConnection conn)
        {
            if (IsClient) return;
            playerManager.UpdateMechaData(packet.Data, conn);
        }
    }
}