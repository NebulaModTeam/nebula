using NebulaModel.Packets.Factory;

namespace NebulaWorld.Factory
{
    public class DestructEntityRequestManager
    {
        public static void DestructEntityRequest(DestructEntityRequest packet)
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
                //Creating rendering batches is required to properly handle DestructFinally for the belts, since model needs to be changed.
                //ToDo: Optimize it somehow, since creating and destroying rendering batches is not optimal.
                planet.factory.cargoTraffic.CreateRenderingBatches();
            }

            FactoryManager.TargetPlanet = packet.PlanetId;
            FactoryManager.PacketAuthor = packet.AuthorId;
            PlanetFactory tmpFactory = pab.factory;
            pab.factory = planet.factory;
            pab.noneTool.factory = planet.factory;

            pab.DoDismantleObject(packet.ObjId);

            pab.factory = tmpFactory;
            pab.noneTool.factory = tmpFactory;
            FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            FactoryManager.PacketAuthor = -1;

            if (packet.PlanetId != GameMain.localPlanet?.id)
            {
                planet.factory.cargoTraffic.DestroyRenderingBatches();
            }
        }
    }
}
