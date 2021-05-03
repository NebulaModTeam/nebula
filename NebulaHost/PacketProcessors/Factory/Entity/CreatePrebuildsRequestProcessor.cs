using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : IPacketProcessor<CreatePrebuildsRequest>
    {
        public void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                Log.Warn($"planet.factory was null create new one");
                planet.factory = GameMain.data.GetOrCreateFactory(planet);
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            if (pab != null)
            {
                FactoryManager.TargetPlanet = packet.PlanetId;

                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = pab.buildPreviews;
                bool tmpConfirm = pab.waitConfirm;
                UnityEngine.Vector3 tmpPos = pab.previewPose.position;
                UnityEngine.Quaternion tmpRot = pab.previewPose.rotation;

                PlanetFactory tmpFactory = null;
                NearColliderLogic tmpNearcdLogic = null;
                PlanetPhysics tmpPlanetPhysics = null;
                float tmpBuildArea = GameMain.mainPlayer.mecha.buildArea;
                PlanetData tmpData = null;
                bool loadExternalPlanetData = GameMain.localPlanet != planet;

                //Load temporary planet data, since host is not there
                if (loadExternalPlanetData)
                {
                    tmpFactory = (PlanetFactory)AccessTools.Field(typeof(PlayerAction_Build), "factory").GetValue(GameMain.mainPlayer.controller.actionBuild);
                    tmpNearcdLogic = (NearColliderLogic)AccessTools.Field(typeof(PlayerAction_Build), "nearcdLogic").GetValue(GameMain.mainPlayer.controller.actionBuild);
                    tmpPlanetPhysics = (PlanetPhysics)AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").GetValue(pab);
                    tmpData = GameMain.mainPlayer.planetData;
                }

                //Create Prebuilds from incomming packet and prepare new position
                pab.buildPreviews = packet.GetBuildPreviews();
                pab.waitConfirm = true;
                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.EventFactory = planet.factory;
                    pab.previewPose.position = new UnityEngine.Vector3(packet.PosePosition.x, packet.PosePosition.y, packet.PosePosition.z);
                    pab.previewPose.rotation = new UnityEngine.Quaternion(packet.PoseRotation.x, packet.PoseRotation.y, packet.PoseRotation.z, packet.PoseRotation.w);

                    //Check if some mandatory variables are missing
                    if (planet.physics == null || planet.physics.colChunks == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }
                    if (planet.aux == null)
                    {
                        planet.aux = new PlanetAuxData(planet);
                    }

                    //Set temporary Local Planet / Factory data that are needed for original methods CheckBuildConditions() and CreatePrebuilds()
                    AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.factory);
                    AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.physics);
                    AccessTools.Field(typeof(PlayerAction_Build), "nearcdLogic").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.physics.nearColliderLogic);
                    AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, planet, null);

                    //Check if prebuilds can be build (collision check, height check, etc)
                    GameMain.mainPlayer.mecha.buildArea = float.MaxValue;
                    bool canBuild;
                    using (FactoryManager.IgnoreBasicBuildConditionChecks.On())
                    {
                        canBuild = pab.CheckBuildConditions();
                        canBuild &= CheckBuildingConnections(pab.buildPreviews, planet.factory.entityPool, planet.factory.prebuildPool);
                    }

                    UnityEngine.Debug.Log(pab.buildPreviews[0].condition);

                    if (canBuild)
                    {
                        FactoryManager.PacketAuthor = packet.AuthorId;
                        CheckAndFixConnections(pab, planet);
                        pab.CreatePrebuilds();
                        FactoryManager.PacketAuthor = -1;
                    }

                    //Revert changes back to the original planet
                    if (loadExternalPlanetData)
                    {
                        planet.physics.Free();
                        planet.physics = null;
                        AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, tmpData, null);
                        AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpPlanetPhysics);
                        AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpFactory);
                        AccessTools.Field(typeof(PlayerAction_Build), "nearcdLogic").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpNearcdLogic);
                    }

                    GameMain.mainPlayer.mecha.buildArea = tmpBuildArea;
                    FactoryManager.EventFactory = null;
                }

                pab.buildPreviews = tmpList;
                pab.waitConfirm = tmpConfirm;
                pab.previewPose.position = tmpPos;
                pab.previewPose.rotation = tmpRot;

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }

        public void CheckAndFixConnections(PlayerAction_Build pab, PlanetData planet)
        {
            //Check and fix references to prebuilds
            Vector3 tmpVector = Vector3.zero;
            foreach (BuildPreview preview in pab.buildPreviews)
            {
                //Check only, if buildPreview has some connection to another prebuild
                if (preview.coverObjId < 0)
                {
                    tmpVector = pab.previewPose.position + pab.previewPose.rotation * preview.lpos;
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

        public bool CheckBuildingConnections(List<BuildPreview> buildPreviews, EntityData[] entityPool, PrebuildData[] prebuildPool)
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
