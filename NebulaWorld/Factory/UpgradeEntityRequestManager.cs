using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class UpgradeEntityRequestManager
    {
        public static void UpgradeEntityRequest(UpgradeEntityRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            PlayerAction_Build pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;

            // We only execute the code if the client has loaded the factory at least once.
            // Else they will get it once they go to the planet for the first time. 
            if (planet?.factory == null || pab == null)
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

            FactoryManager.TargetPlanet = packet.PlanetId;
            FactoryManager.PacketAuthor = packet.AuthorId;
            PlanetFactory tmpFactory = pab.factory;
            pab.factory = planet.factory;
            pab.noneTool.factory = planet.factory;

            pab.DoUpgradeObject(packet.ObjId, packet.Grade, packet.UpgradeProtoId, out int _);

            pab.factory = tmpFactory;
            pab.noneTool.factory = tmpFactory;
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
