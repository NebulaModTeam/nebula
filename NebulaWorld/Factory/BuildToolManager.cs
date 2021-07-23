using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class BuildToolManager
    {
        public static void CreatePrebuildsRequest(CreatePrebuildsRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                if (FactoryManager.EventFromServer)
                {
                    // We only execute the code if the client has loaded the factory at least once.
                    // Else it will get it once it goes to the planet for the first time. 
                    return;
                }
                Log.Warn($"planet.factory was null create new one");
                planet.factory = GameMain.data.GetOrCreateFactory(planet);
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;
            BuildTool[] buildTools = pab.tools;
            BuildTool buildTool = null;
            for (int i = 0; i < buildTools.Length; i++)
            {
                if (buildTools[i].GetType().ToString() == packet.BuildToolType)
                {
                    buildTool = buildTools[i];
                    break;
                }
            }

            if (pab != null && buildTool != null)
            {
                FactoryManager.TargetPlanet = packet.PlanetId;
                FactoryManager.PacketAuthor = packet.AuthorId;

                PlanetFactory tmpFactory = null;
                NearColliderLogic tmpNearcdLogic = null;
                PlanetPhysics tmpPlanetPhysics = null;
                bool loadExternalPlanetData = GameMain.localPlanet?.id != planet.id;

                if (loadExternalPlanetData)
                {
                    //Make backup of values that are overwritten
                    tmpFactory = buildTool.factory;
                    tmpNearcdLogic = buildTool.actionBuild.nearcdLogic;
                    tmpPlanetPhysics = buildTool.actionBuild.planetPhysics;
                    FactoryManager.AddPlanetTimer(packet.PlanetId);
                }

                bool incomingBlueprintEvent = packet.BuildToolType == typeof(BuildTool_BlueprintPaste).ToString();

                //Create Prebuilds from incoming packet and prepare new position
                List<BuildPreview> tmpList = new List<BuildPreview>();
                if (!incomingBlueprintEvent)
                {
                    tmpList.AddRange(buildTool.buildPreviews);
                    buildTool.buildPreviews.Clear();
                    buildTool.buildPreviews.AddRange(packet.GetBuildPreviews());
                }

                FactoryManager.EventFactory = planet.factory;

                //Set temporary Local Planet / Factory data that are needed for original methods CheckBuildConditions() and CreatePrebuilds()
                buildTool.factory = planet.factory;
                pab.factory = planet.factory;
                pab.noneTool.factory = planet.factory;
                if (FactoryManager.EventFromClient)
                {
                    // Only the server needs to set these
                    pab.planetPhysics = planet.physics;
                    pab.nearcdLogic = planet.physics.nearColliderLogic;
                }

                //Check if prebuilds can be build (collision check, height check, etc)
                bool canBuild = false;
                if (FactoryManager.EventFromClient)
                {
                    GameMain.mainPlayer.mecha.buildArea = float.MaxValue;
                    canBuild = CheckBuildingConnections(buildTool.buildPreviews, planet.factory.entityPool, planet.factory.prebuildPool);
                }

                if (canBuild || FactoryManager.EventFromServer)
                {
                    if (FactoryManager.EventFromClient) CheckAndFixConnections(buildTool, planet);

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
                    else if (incomingBlueprintEvent)
                    {
                        // TODO: Do we need to cache and revert bpCursor and bpTool to what it was after we've done this ?
                        List<BuildPreview> incomingPreviews = packet.GetBuildPreviews();
                        BuildTool_BlueprintPaste bpTool = buildTool as BuildTool_BlueprintPaste;
                        bpTool.bpCursor = incomingPreviews.Count;
                        bpTool.bpPool = incomingPreviews.ToArray();
                        bpTool.CreatePrebuilds();
                    }
                }

                //Revert changes back to the original planet
                if (loadExternalPlanetData)
                {
                    buildTool.factory = tmpFactory;
                    pab.factory = tmpFactory;
                    pab.noneTool.factory = tmpFactory;
                    pab.planetPhysics = tmpPlanetPhysics;
                    pab.nearcdLogic = tmpNearcdLogic;
                }

                GameMain.mainPlayer.mecha.buildArea = Configs.freeMode.mechaBuildArea;
                FactoryManager.EventFactory = null;

                if (!incomingBlueprintEvent)
                {
                    buildTool.buildPreviews.Clear();
                    buildTool.buildPreviews.AddRange(tmpList);
                }

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                FactoryManager.PacketAuthor = -1;
            }
        }

        public static void CheckAndFixConnections(BuildTool buildTool, PlanetData planet)
        {
            foreach (BuildPreview preview in buildTool.buildPreviews)
            {
                //Check only, if buildPreview has some connection to another prebuild
                if (preview.coverObjId < 0)
                {
                    Vector3 tmpVector = preview.lpos;
                    if (planet.factory.prebuildPool[-preview.coverObjId].id != 0)
                    {
                        //Prebuild exists, check if it is same prebuild that client wants by comparing prebuild positions
                        if (tmpVector == planet.factory.prebuildPool[-preview.coverObjId].pos)
                        {
                            //Position of prebuilds are same, everything is OK.
                            continue;
                        }
                    }
                    // Prebuild does not exists, check what is the new ID of the finished building that was constructed from prebuild
                    // or
                    // Positions of prebuilds are different, which means this is different prebuild and we need to find ID of contructed building
                    foreach (EntityData entity in planet.factory.entityPool)
                    {
                        // `entity.pos == tmpVector` does not work in every cases (rounding errors?).
                        if ((entity.pos - tmpVector).sqrMagnitude < 0.1f)
                        {
                            preview.coverObjId = entity.id;
                            break;
                        }
                    }
                }
            }
        }

        public static bool CheckBuildingConnections(List<BuildPreview> buildPreviews, EntityData[] entityPool, PrebuildData[] prebuildPool)
        {
            //Check if some entity that is suppose to be connected to this building is missing
            for (int i = 0; i < buildPreviews.Count; i++)
            {
                var buildPreview = buildPreviews[i];
                int inputObjId = buildPreview.inputObjId;
                if (inputObjId > 0)
                {
                    if (inputObjId >= entityPool.Length || entityPool[inputObjId].id == 0)
                    {
                        return false;
                    }
                }
                else if (inputObjId < 0)
                {
                    inputObjId = -inputObjId;
                    if (inputObjId >= prebuildPool.Length || prebuildPool[inputObjId].id == 0)
                    {
                        return false;
                    }
                }

                int outputObjId = buildPreview.outputObjId;
                if (outputObjId > 0)
                {
                    if (outputObjId >= entityPool.Length || entityPool[outputObjId].id == 0)
                    {
                        return false;
                    }
                }
                else if (outputObjId < 0)
                {
                    outputObjId = -outputObjId;
                    if (outputObjId >= prebuildPool.Length || prebuildPool[outputObjId].id == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
