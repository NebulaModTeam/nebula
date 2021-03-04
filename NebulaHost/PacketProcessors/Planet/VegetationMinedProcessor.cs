using LiteNetLib;
using NebulaHost.MonoBehaviours;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class VegetationMinedProcessor : IPacketProcessor<VegeMined>
    {
        private PlayerManager playerManager;

        public VegetationMinedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance.PlayerManager;
        }

        public void ProcessPacket(VegeMined packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.ReliableUnordered);

            // TODO: Should update the host state immediatly here
        }
    }
}
