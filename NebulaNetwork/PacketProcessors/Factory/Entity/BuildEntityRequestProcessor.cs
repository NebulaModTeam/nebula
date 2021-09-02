using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class BuildEntityRequestProcessor : PacketProcessor<BuildEntityRequest>
    {
        public override void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
        {
            if (IsHost && !Multiplayer.Session.Factories.ContainsPrebuildRequest(packet.PlanetId, packet.PrebuildId))
            {
                Log.Warn($"BuildEntityRequest received does not have a corresponding PrebuildRequest with the id {packet.PrebuildId} for the planet {packet.PlanetId}");
                return;
            }

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (Multiplayer.Session.Factories.IsIncomingRequest.On())
                {
                    Multiplayer.Session.Factories.EventFactory = planet.factory;
                    Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
                    Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

                    Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);

                    //Remove building from drone queue
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(-packet.PrebuildId);
                    planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);

                    if (IsClient)
                    {
                        Multiplayer.Session.Drones.RemoveBuildRequest(-packet.PrebuildId);
                    }

                    Multiplayer.Session.Factories.EventFactory = null;
                    Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
                    Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
                }
            }
        }
    }
}
