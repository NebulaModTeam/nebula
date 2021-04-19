using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class AddEntityPreviewRequestProcessor : IPacketProcessor<AddEntityPreviewRequest>
    {
        public void ProcessPacket(AddEntityPreviewRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (FactoryManager.EventFromServer.On())
                {
                    PrebuildData prebuild = packet.GetPrebuildData();
                    int localPlanetId = GameMain.localPlanet?.id ?? -1;
                    if (packet.PlanetId == localPlanetId)
                    {
                        planet.factory.AddPrebuildDataWithComponents(prebuild);
                    }
                    else
                    {
                        planet.factory.AddPrebuildData(prebuild);
                    }
                }
            }
        }
    }
}
