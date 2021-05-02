using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class UpgradeEntityRequestProcessor : IPacketProcessor<UpgradeEntityRequest>
    {
        public void ProcessPacket(UpgradeEntityRequest packet, NebulaConnection conn)
        {
            using (FactoryManager.EventFromClient.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

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
}
