using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using FactoryManager = NebulaWorld.Factory.FactoryManager;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : PacketProcessor<UpgradeEntityRequest>
    {
        public override void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            using (FactoryManager.Instance.IsIncomingRequest.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

                // We only execute the code if the client has loaded the factory at least once.
                // Else they will get it once they go to the planet for the first time. 
                if (planet?.factory == null)
                {
                    return;
                }

                FactoryManager.Instance.TargetPlanet = packet.PlanetId;
                FactoryManager.Instance.PacketAuthor = packet.AuthorId;

                FactoryManager.Instance.AddPlanetTimer(packet.PlanetId);
                ItemProto itemProto = LDB.items.Select(packet.UpgradeProtoId);
                planet.factory.UpgradeFinally(GameMain.mainPlayer, packet.ObjId, itemProto);

                FactoryManager.Instance.TargetPlanet = NebulaModAPI.PLANET_NONE;
                FactoryManager.Instance.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            }
        }
    }
}
