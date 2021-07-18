using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class UpgradeEntityRequestManager
    {
        public static void UpgradeEntityRequest(UpgradeEntityRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else he will get it once it goes to the planet for the first time. 
            if (planet.factory == null)
            {
                return;
            }

            FactoryManager.AddPlanetTimer(packet.PlanetId);

            ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
            FactoryManager.TargetPlanet = packet.PlanetId;
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);
            FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
        }
    }
}
