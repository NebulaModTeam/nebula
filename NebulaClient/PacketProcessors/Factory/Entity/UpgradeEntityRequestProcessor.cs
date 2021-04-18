using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : IPacketProcessor<UpgradeEntityRequest>
    {
        public void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else he will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (FactoryManager.EventFromServer.On())
                {
                    ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
                    planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);
                }
            }
        }
    }
}
