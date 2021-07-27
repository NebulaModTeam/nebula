﻿using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using NebulaWorld.Player;

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
                    FactoryManager.TargetPlanet = packet.PlanetId;
                    FactoryManager.PacketAuthor = packet.AuthorId;

                    FactoryManager.AddPlanetTimer(packet.PlanetId);

                    //Remove building from drone queue
                    GameMain.mainPlayer.mecha.droneLogic.serving.Remove(-packet.PrebuildId);
                    planet.factory.BuildFinally(GameMain.mainPlayer, packet.PrebuildId);
                    DroneManager.RemoveBuildRequest(-packet.PrebuildId);

                    FactoryManager.PacketAuthor = -1;
                    FactoryManager.EventFactory = null;
                    FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                }
            }
        }
    }
}
