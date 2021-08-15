using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipItemsProcessor : PacketProcessor<ILSShipItems>
    {
        private PlayerManager playerManager;
        public ILSShipItemsProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }
        public override void ProcessPacket(ILSShipItems packet, NebulaConnection conn)
        {
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            // TODO: Shouldn't we call this also on host ??
            if (IsClient)
            {
                SimulatedWorld.OnILSShipItemsUpdate(packet);
            }
        }
    }
}
