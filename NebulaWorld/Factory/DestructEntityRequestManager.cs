using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class DestructEntityRequestManager
    {
        public static void DestructEntityRequest(DestructEntityRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory == null)
            {
                return;
            }

            FactoryManager.TargetPlanet = packet.PlanetId;
            FactoryManager.PacketAuthor = packet.AuthorId;

            FactoryManager.AddPlanetTimer(packet.PlanetId);
            int protoId = packet.ProtoId;
            planet.factory.DismantleFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);

            FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            FactoryManager.PacketAuthor = -1;
        }
    }
}
