using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : IPacketProcessor<CreatePrebuildsRequest>
    {
        public void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                // We only execute the code if the client has loaded the factory at least once.
                // Else it will get it once it goes to the planet for the first time. 
                return;
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            BuildTool[] buildTools = GameMain.mainPlayer.controller?.actionBuild.tools;
            BuildTool buildTool = null;
            for (int i = 0; i < buildTools.Length; i++)
            {
                if (buildTools[i].GetType().ToString() == packet.BuildToolType)
                    buildTool = buildTools[i];
            }
            if (pab != null && buildTool != null)
            {
                FactoryManager.TargetPlanet = packet.PlanetId;
                FactoryManager.PacketAuthor = packet.AuthorId;

                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = new List<BuildPreview>();
                PlanetFactory tmpFactory = buildTool.factory;
                PlanetPhysics tmpPlanetPhysics = pab.planetPhysics;

                //Create Prebuilds from incoming packet
                tmpList.AddRange(buildTool.buildPreviews);
                buildTool.buildPreviews.Clear();
                buildTool.buildPreviews.AddRange(packet.GetBuildPreviews());

                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.EventFactory = planet.factory;
                    buildTool.factory = planet.factory;
                    pab.factory = planet.factory;
                    pab.noneTool.factory = planet.factory;
                    AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, planet, null);

                    //Create temporary physics for spawning building's colliders
                    if (planet.physics == null || planet.physics.colChunks == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }

                    AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.physics);

                    if (packet.BuildToolType == typeof(BuildTool_Click).ToString())
                    {
                        ((BuildTool_Click)buildTool).CreatePrebuilds();
                    }
                    else if (packet.BuildToolType == typeof(BuildTool_Path).ToString())
                    {
                        ((BuildTool_Path)buildTool).CreatePrebuilds();
                    }
                    else if (packet.BuildToolType == typeof(BuildTool_Inserter).ToString())
                    {
                        ((BuildTool_Inserter)buildTool).CreatePrebuilds();
                    }

                    FactoryManager.EventFactory = null;
                }

                // TODO: FIX THE BELOW

                //Author has to call this for the continuous belt building
/*                if (packet.AuthorId == LocalPlayer.PlayerId)
                {
                    pab.AfterPrebuild();
                }*/

                /* TODO: Oh *boy* is this one going to be a fun one to fix - they've actually refactored the AfterPrebuild method
                 * to happen at each of the relevant times the cmd.mode condition would have been met in every tool 
                 *
                 * Cod's Suggestion: Transpile everything :) */

                //Revert changes back
                buildTool.factory = tmpFactory;
                pab.factory = tmpFactory;
                pab.noneTool.factory = tmpFactory;
                AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, tmpData, null);

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                FactoryManager.PacketAuthor = -1;
            }
        }
    }
}
