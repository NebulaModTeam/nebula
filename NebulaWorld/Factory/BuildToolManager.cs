using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Packets.Factory;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaWorld.Factory
{
    public class BuildToolManager : IDisposable
    {
        public const long WAIT_TIME = 10000;
        public Vector3 LastPosition;
        public long LastCheckTime;

        public BuildToolManager()
        {
        }

        public void Dispose()
        {
        }

        public void CreatePrebuildsRequest(CreatePrebuildsRequest packet)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                if (Multiplayer.Session.Factories.IsIncomingRequest.Value)
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
                Multiplayer.Session.Factories.TargetPlanet = packet.PlanetId;
                Multiplayer.Session.Factories.PacketAuthor = packet.AuthorId;

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
                    Multiplayer.Session.Factories.AddPlanetTimer(packet.PlanetId);
                }                

                bool incomingBlueprintEvent = packet.BuildToolType == typeof(BuildTool_BlueprintPaste).ToString();
                Vector3 pos = Vector3.zero;

                //Create Prebuilds from incoming packet and prepare new position
                List<BuildPreview> tmpList = new List<BuildPreview>();
                if (!incomingBlueprintEvent)
                {
                    tmpList.AddRange(buildTool.buildPreviews);
                    buildTool.buildPreviews.Clear();
                    buildTool.buildPreviews.AddRange(packet.GetBuildPreviews());
                    pos = buildTool.buildPreviews[0].lpos;
                }

                Multiplayer.Session.Factories.EventFactory = planet.factory;

                //Set temporary Local Planet / Factory data that are needed for original methods CheckBuildConditions() and CreatePrebuilds()
                buildTool.factory = planet.factory;
                pab.factory = planet.factory;
                pab.noneTool.factory = planet.factory;
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    // Only the server needs to set these
                    pab.planetPhysics = planet.physics;
                    pab.nearcdLogic = planet.physics.nearColliderLogic;
                }

                //Check if prebuilds can be build (collision check, height check, etc)
                bool canBuild = false;
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    GameMain.mainPlayer.mecha.buildArea = float.MaxValue;
                    canBuild = CheckBuildingConnections(buildTool.buildPreviews, planet.factory.entityPool, planet.factory.prebuildPool);
                    if (!canBuild)
                    {
                        Log.Warn($"CreatePrebuildsRequest: request do not pass connections test on planet {planet.id}");
                    }
                }

                if (canBuild || Multiplayer.Session.LocalPlayer.IsClient)
                {
                    if (Multiplayer.Session.Factories.IsIncomingRequest.Value)
                    {
                        CheckAndFixConnections(buildTool, planet);
                    }
                    if (Multiplayer.Session.LocalPlayer.IsClient)
                    {
                        if (packet.PrebuildId != Multiplayer.Session.Factories.GetNextPrebuildId(packet.PlanetId))
                        {
                            string warningText = $"PrebuildId mismatch on {packet.PlanetId} planet: {packet.PrebuildId} != {Multiplayer.Session.Factories.GetNextPrebuildId(planet.factory)}";
                            Log.WarnInform(warningText + ". Consider reconnecting");
                            NebulaWorld.Warning.WarningManager.DisplayTemporaryWarning(warningText, 5000);
                        }
                    }

                    if (packet.BuildToolType == typeof(BuildTool_Click).ToString())
                    {
                        ((BuildTool_Click)buildTool).CreatePrebuilds();
                    }
                    else if (packet.BuildToolType == typeof(BuildTool_Path).ToString())
                    {
                        ((BuildTool_Path)buildTool).CreatePrebuilds();
                    }
                    else if(packet.BuildToolType == typeof(BuildTool_PathAddon).ToString())
                    {
                        ((BuildTool_PathAddon)buildTool).handbp = buildTool.buildPreviews[0]; // traffic monitors cannot be drag build atm, so its always only one.
                        ((BuildTool_PathAddon)buildTool).CreatePrebuilds();
                    }
                    else if (packet.BuildToolType == typeof(BuildTool_Inserter).ToString())
                    {
                        ((BuildTool_Inserter)buildTool).CreatePrebuilds();
                    }
                    else if (incomingBlueprintEvent)
                    {
                        BuildTool_BlueprintPaste bpTool = buildTool as BuildTool_BlueprintPaste;

                        // Cache the current data before performing the requested CreatePrebuilds();
                        int previousCursor = bpTool.bpCursor;
                        BuildPreview[] previousPool = bpTool.bpPool;

                        // Perform the requested CreatePrebuilds();
                        List<BuildPreview> incomingPreviews = packet.GetBuildPreviews();
                        bpTool.bpCursor = incomingPreviews.Count;
                        bpTool.bpPool = incomingPreviews.ToArray();
                        bpTool.CreatePrebuilds();
                        pos = incomingPreviews[0].lpos;

                        // Revert to previous data
                        bpTool.bpCursor = previousCursor;
                        bpTool.bpPool = previousPool;
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
                Multiplayer.Session.Factories.EventFactory = null;

                if (!incomingBlueprintEvent)
                {
                    buildTool.buildPreviews.Clear();
                    buildTool.buildPreviews.AddRange(tmpList);
                }

                Multiplayer.Session.Factories.TargetPlanet = NebulaModAPI.PLANET_NONE;
                Multiplayer.Session.Factories.PacketAuthor = NebulaModAPI.AUTHOR_NONE;

                if (pos == LastPosition)
                {
                    //Reset check timer on client
                    LastCheckTime = 0;
                }
            }
        }

        public void CheckAndFixConnections(BuildTool buildTool, PlanetData planet)
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
                            Log.Info($"CheckAndFixConnections: {entity.pos} {tmpVector}");
                            preview.coverObjId = entity.id;
                            break;
                        }
                    }
                }
            }
        }

        public bool CheckBuildingConnections(List<BuildPreview> buildPreviews, EntityData[] entityPool, PrebuildData[] prebuildPool)
        {
            //Check if some entity that is suppose to be connected to this building is missing
            for (int i = 0; i < buildPreviews.Count; i++)
            {
                BuildPreview buildPreview = buildPreviews[i];
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
        
        public bool InitialCheck(Vector3 pos)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if ((now - LastCheckTime) < WAIT_TIME && LastPosition == pos)
            {
                //Stop client from sending prebuilds at the same position
                UIRealtimeTip.Popup("Please wait for server respond");
                return false;
            }
            LastCheckTime = now;
            LastPosition = pos;
            return true;
        }
    }
}
