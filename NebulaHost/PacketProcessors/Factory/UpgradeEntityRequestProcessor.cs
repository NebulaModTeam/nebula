using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : IPacketProcessor<UpgradeEntityRequest>
    {
        public void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            FactoryManager.EventFromClient = true;
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);

            FactoryManager.EventFromClient = false;
        }
    }
}
