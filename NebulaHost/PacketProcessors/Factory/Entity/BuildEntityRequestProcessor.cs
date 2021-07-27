using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    public class BuildEntityRequestProcessor : IPacketProcessor<BuildEntityRequest>
    {
        public void ProcessPacket(BuildEntityRequest packet, NebulaConnection conn)
        {
            if (!FactoryManager.ContainsPrebuildRequest(packet.PlanetId, packet.PrebuildId))
            {
                Log.Warn($"BuildEntityRequest received does not have a corresponding PrebuildRequest with the id {packet.PrebuildId} for the planet {packet.PlanetId}");
                return;
            }

            using (FactoryManager.EventFromClient.On())
            {
                PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
                FactoryManager.EventFactory = planet.factory;
                FactoryManager.PacketAuthor = packet.AuthorId;
                FactoryManager.TargetPlanet = packet.PlanetId;

                FactoryManager.AddPlanetTimer(packet.PlanetId);

                //Remove building from drone queue
                GameMain.mainPlayer.mecha.droneLogic.serving.Remove(-packet.PrebuildId);
                planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);

                FactoryManager.EventFactory = null;
                FactoryManager.PacketAuthor = -1;
                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
