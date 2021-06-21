using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : IPacketProcessor<DestructEntityRequest>
    {
        public void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                int protoId = 0;
                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.PacketAuthor = packet.AuthorId;
                    FactoryManager.TargetPlanet = packet.PlanetId;
                    planet.factory.DismantleFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);
                    FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                    FactoryManager.PacketAuthor = -1;
                }
            }
        }
    }
}
