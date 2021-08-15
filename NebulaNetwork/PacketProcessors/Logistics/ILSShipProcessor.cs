using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    public class ILSShipProcessor : PacketProcessor<ILSShipData>
    {
        private PlayerManager playerManager;
        public ILSShipProcessor()
        {
            playerManager = MultiplayerHostSession.Instance?.PlayerManager;
        }

        public override void ProcessPacket(ILSShipData packet, NebulaConnection conn)
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
                using (FactoryManager.IsIncomingRequest.On())
                {
                    SimulatedWorld.OnILSShipUpdate(packet);
                }
            }
        }
    }
}
