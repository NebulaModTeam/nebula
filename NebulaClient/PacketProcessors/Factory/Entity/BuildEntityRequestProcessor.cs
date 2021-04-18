using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class BuildEntityRequestProcessor : IPacketProcessor<BuildEntityRequest>
    {
        public void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.EventFactory = planet.factory;

                    // Physics could be null, if the host is not on the requested planet
                    if (packet.PlanetId != GameMain.localPlanet?.id)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();

                        planet.audio = new PlanetAudio(planet);
                        planet.audio.Init();
                    }

                    planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);

                    // Make sure to free the physics once the FlattenTerrain is done
                    if (packet.PlanetId != GameMain.localPlanet?.id)
                    {
                        planet.physics.Free();
                        planet.physics = null;

                        planet.audio.Free();
                        planet.audio = null;
                    }
                    FactoryManager.EventFactory = null;
                }
            }
        }
    }
}
