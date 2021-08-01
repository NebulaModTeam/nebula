using NebulaModel.Attributes;
using Mirror;
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
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(PlayerMechaData packet, NetworkConnection conn)
        {
            if (IsClient) return;
            playerManager.UpdateMechaData(packet.Data, conn);
        }
    }
}