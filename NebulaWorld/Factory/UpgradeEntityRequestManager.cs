using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class UpgradeEntityRequestManager
    {
        public static void UpgradeEntityRequest(UpgradeEntityRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null)
            {
                return;
            }

            FactoryManager.TargetPlanet = packet.PlanetId;
            FactoryManager.PacketAuthor = packet.AuthorId;

            FactoryManager.AddPlanetTimer(packet.PlanetId);
            ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);

            FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            FactoryManager.PacketAuthor = -1;
        }
    }
}
