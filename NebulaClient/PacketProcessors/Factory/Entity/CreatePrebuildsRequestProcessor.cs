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
                    pab.planetPhysics = planet.physics;

                    //Create temporary physics for spawning building's colliders
                    if (planet.physics == null || planet.physics.colChunks == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }

                    //Take item from the inventory if player is author of the build
                    if (packet.AuthorId == LocalPlayer.PlayerId)
                    {
                        foreach (BuildPreview buildPreview in buildTool.buildPreviews)
                        {
                            if (GameMain.mainPlayer.inhandItemId == buildPreview.item.ID && GameMain.mainPlayer.inhandItemCount > 0)
                            {
                                GameMain.mainPlayer.UseHandItems(1);
                            }
                            else
                            {
                                int num = 1;
                                GameMain.mainPlayer.package.TakeTailItems(ref buildPreview.item.ID, ref num, false);
                            }
                        }
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
                pab.planetPhysics = tmpPlanetPhysics;
                buildTool.factory = tmpFactory;
                pab.factory = tmpFactory;
                buildTool.buildPreviews.Clear();
                buildTool.buildPreviews.AddRange(tmpList);

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
