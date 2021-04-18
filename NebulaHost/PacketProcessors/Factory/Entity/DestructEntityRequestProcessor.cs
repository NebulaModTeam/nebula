using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : IPacketProcessor<DestructEntityRequest>
    {
        public void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            int protoId = 0;
            using (FactoryManager.EventFromClient.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                using (FactoryManager.DoNotAddItemsFromBuildingOnDestruct.On())
                {
                    FactoryManager.PacketAuthor = packet.AuthorId;
                    planet.factory.DestructFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);
                    FactoryManager.PacketAuthor = -1;
                }
            }
        }
    }
}
