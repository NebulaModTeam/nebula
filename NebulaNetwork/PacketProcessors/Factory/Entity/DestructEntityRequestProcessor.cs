using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using FactoryManager = NebulaWorld.Factory.FactoryManager;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class DestructEntityRequestProcessor : PacketProcessor<DestructEntityRequest>
    {
        public override void ProcessPacket(DestructEntityRequest packet, NebulaConnection conn)
        {
            using (FactoryManager.Instance.IsIncomingRequest.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                PlayerAction_Build pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;

                // We only execute the code if the client has loaded the factory at least once.
                // Else they will get it once they go to the planet for the first time. 
                if (planet?.factory == null || pab == null)
                {
                    return;
                }

                FactoryManager.Instance.TargetPlanet = packet.PlanetId;
                FactoryManager.Instance.PacketAuthor = packet.AuthorId;
                PlanetFactory tmpFactory = pab.factory;
                pab.factory = planet.factory;
                pab.noneTool.factory = planet.factory;

                FactoryManager.Instance.AddPlanetTimer(packet.PlanetId);
                pab.DoDismantleObject(packet.ObjId);

                pab.factory = tmpFactory;
                pab.noneTool.factory = tmpFactory;
                FactoryManager.Instance.TargetPlanet = NebulaModAPI.PLANET_NONE;
                FactoryManager.Instance.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
            }
        }
    }
}
