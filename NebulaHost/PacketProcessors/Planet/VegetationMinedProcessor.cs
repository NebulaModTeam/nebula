using LiteNetLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using NebulaWorld;

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

            SimulatedWorld.MineVegetable(packet);
        }
    }
}
