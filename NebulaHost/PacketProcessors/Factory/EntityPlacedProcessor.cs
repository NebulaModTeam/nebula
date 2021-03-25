using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    public class EntityPlacedProcessor : IPacketProcessor<EntityPlaced>
    {
        private PlayerManager playerManager;
        public void ProcessPacket(EntityPlaced packet, NebulaConnection conn)
        {
            Player player = PlayerManager.GetPlayer(conn);
            if(player != null)
            {
                PlayerManager.SendPacketToOtherPlayers(packet, player);
            }
            SimulatedWorld.OnPlaceEntity(packet);
        }
    }
}
