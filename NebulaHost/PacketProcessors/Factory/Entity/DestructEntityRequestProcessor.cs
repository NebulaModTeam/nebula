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
            using (FactoryManager.EventFromClient.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                FactoryManager.PacketAuthor = packet.AuthorId;
                FactoryManager.TargetPlanet = packet.PlanetId;
                if (packet.PlanetId != GameMain.mainPlayer.planetId)
                {
                    //Creating rendering batches is required to properly handle DestructFinally for the belts, since model needs to be changed.
                    //ToDo: Optimize it somehow, since creating and destroying rendering batches is not optimal.
                    planet.factory.cargoTraffic.CreateRenderingBatches();
                }

                var pab = GameMain.mainPlayer.controller.actionBuild;
                if (pab != null)
                {
                    // Backup current factory & set factory to request planet factory
                    var tmpFactory = pab.planet.factory;
                    pab.planet.factory = planet.factory;
                    pab.noneTool.factory = planet.factory;

                    pab.DoDismantleObject(packet.ObjId);

                    // Restore factory
                    pab.planet.factory = tmpFactory;
                    pab.noneTool.factory = tmpFactory;
                }

                if (packet.PlanetId != GameMain.mainPlayer.planetId)
                {
                    planet.factory.cargoTraffic.DestroyRenderingBatches();
                }
                FactoryManager.PacketAuthor = -1;
                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
