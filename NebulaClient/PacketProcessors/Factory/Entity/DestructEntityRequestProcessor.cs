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
                using (FactoryManager.DoNotAddItemsFromBuildingOnDestruct.On(packet.AuthorId != LocalPlayer.PlayerId))
                {
                    if (packet.AuthorId == LocalPlayer.PlayerId)
                    {
                        //I am author so I will take item as a building
                        PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
                        if (pab != null)
                        {
                            int itemId = (packet.ObjId > 0 ? LDB.items.Select((int)planet.factory.entityPool[packet.ObjId].protoId) : LDB.items.Select((int)planet.factory.prebuildPool[-packet.ObjId].protoId))?.ID ?? -1;
                            //Todo: Check for the full accumulator building
                            if (itemId != -1)
                            {
                                GameMain.mainPlayer.TryAddItemToPackage(itemId, 1, true, packet.ObjId);
                                UIItemup.Up(itemId, 1);
                            }
                        }
                    }
                    FactoryManager.TargetPlanet = packet.PlanetId;
                    planet.factory.DestructFinally(GameMain.mainPlayer, packet.ObjId, ref protoId);
                    FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                }
            }
        }
    }
}
