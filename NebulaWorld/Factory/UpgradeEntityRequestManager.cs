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

            // Physics could be null, if the host is not on the requested planet
            if (packet.PlanetId != GameMain.localPlanet?.id)
            {
                planet.physics = new PlanetPhysics(planet);
                planet.physics.Init();
                planet.audio = new PlanetAudio(planet);
                planet.audio.Init();
            }

            ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
            FactoryManager.TargetPlanet = packet.PlanetId;
            planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);
            FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;

            if (packet.PlanetId != GameMain.localPlanet?.id)
            {
                planet.physics.Free();
                planet.physics = null;
                planet.audio.Free();
                planet.audio = null;
            }
        }
    }
}
