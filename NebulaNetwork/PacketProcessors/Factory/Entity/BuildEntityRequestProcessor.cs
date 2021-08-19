using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory;
using NebulaWorld.Player;
using FactoryManager = NebulaWorld.Factory.FactoryManager;

namespace NebulaNetwork.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class BuildEntityRequestProcessor : PacketProcessor<BuildEntityRequest>
    {
        public override void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
        {
            if (IsHost && !FactoryManager.Instance.ContainsPrebuildRequest(packet.PlanetId, packet.PrebuildId))
            {
                Log.Warn($"BuildEntityRequest received does not have a corresponding PrebuildRequest with the id {packet.PrebuildId} for the planet {packet.PlanetId}");
                return;
            }

            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);

            // We only execute the code if the client has loaded the factory at least once.
            // Else it will get it once it goes to the planet for the first time. 
            if (planet.factory != null)
            {
                using (FactoryManager.Instance.IsIncomingRequest.On())
                {
                    FactoryManager.Instance.EventFactory = planet.factory;
                    FactoryManager.Instance.TargetPlanet = packet.PlanetId;
                    FactoryManager.Instance.PacketAuthor = packet.AuthorId;

                    FactoryManager.Instance.AddPlanetTimer(packet.PlanetId);

                    //Remove building from drone queue
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(-packet.PrebuildId);
                    planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);

                    if (IsClient)
                    {
                        DroneManager.RemoveBuildRequest(-packet.PrebuildId);
                    }

                    FactoryManager.Instance.EventFactory = null;
                    FactoryManager.Instance.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
                    FactoryManager.Instance.TargetPlanet = NebulaModAPI.PLANET_NONE;
                }
            }
        }
    }
}
