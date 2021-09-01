using NebulaAPI;
using NebulaModel;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    class PlayerMechaDataProcessor : PacketProcessor<PlayerMechaData>
    {
        private IPlayerManager playerManager;

        public PlayerMechaDataProcessor()
        {
            playerManager = Multiplayer.Session.Network.PlayerManager;
        }

        public override void ProcessPacket(PlayerMechaData packet, NebulaConnection conn)
        {
            if (IsClient) return;
            playerManager.UpdateMechaData(packet.Data, conn);
        }
    }
}