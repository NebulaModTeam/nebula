using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld.Factory;
using NebulaWorld.Player;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class BuildEntityRequestProcessor : PacketProcessor<BuildEntityRequest>
    {
        public override void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
        {
            if (IsHost && !FactoryManager.ContainsPrebuildRequest(packet.PlanetId, packet.PrebuildId))
            {
                Log.Warn($"BuildEntityRequest received does not have a corresponding PrebuildRequest with the id {packet.PrebuildId} for the planet {packet.PlanetId}");
                return;
            }

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (FactoryManager.IsIncomingRequest.On())
                {
                    FactoryManager.EventFactory = planet.factory;
                    FactoryManager.TargetPlanet = packet.PlanetId;
                    FactoryManager.PacketAuthor = packet.AuthorId;

                    FactoryManager.AddPlanetTimer(packet.PlanetId);

                    //Remove building from drone queue
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(-packet.PrebuildId);
                    planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);

                    if (IsClient)
                    {
                        DroneManager.RemoveBuildRequest(-packet.PrebuildId);
                    }

                    FactoryManager.EventFactory = null;
                    FactoryManager.PacketAuthor = FactoryManager.AUTHOR_NONE;
                    FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                }
            }
        }
    }
}
