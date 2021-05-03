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
                    FactoryManager.TargetPlanet = packet.PlanetId;
                    if (packet.PlanetId != GameMain.mainPlayer.planetId)
                    {
                        //Creating rendering batches is required to properly handle DestructFinally for the belts, since model needs to be changed.
                        //ToDo: Optimize it somehow, since creating and destroying rendering batches is not optimal.
                        planet.factory.cargoTraffic.CreateRenderingBatches();
                    }
                    planet.factory.DestructFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);
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
}
