using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using System.Collections.Generic;

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
                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = pab.buildPreviews;
                bool tmpConfirm = pab.waitConfirm;
                UnityEngine.Vector3 tmpPos = pab.previewPose.position;
                UnityEngine.Quaternion tmpRot = pab.previewPose.rotation;

                PlanetFactory tmpFactory = null;
                NearColliderLogic tmpNearcdLogic = null;
                PlanetPhysics tmpPlanetPhysics = null;
                float tmpBuildArea = 0f;
                PlanetData tmpData = null;
                bool loadExternalPlanetData = GameMain.localPlanet != planet;

                //Load temporary planet data, since host is not there
                if (loadExternalPlanetData)
                {
                    tmpFactory = (PlanetFactory)AccessTools.Field(typeof(PlayerAction_Build), "factory").GetValue(GameMain.mainPlayer.controller.actionBuild);
                    tmpNearcdLogic = (NearColliderLogic)AccessTools.Field(typeof(PlayerAction_Build), "nearcdLogic").GetValue(GameMain.mainPlayer.controller.actionBuild);
                    tmpPlanetPhysics = (PlanetPhysics)AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").GetValue(pab);
                    tmpBuildArea = GameMain.mainPlayer.mecha.buildArea;
                    tmpData = GameMain.mainPlayer.planetData;
                }
                //Create Prebuilds from incomming packet and prepare new position
                pab.buildPreviews = packet.GetBuildPreviews();
                pab.waitConfirm = true;
                FactoryManager.EventFromServer = true;
                FactoryManager.EventFactory = planet.factory;
                pab.previewPose.position = new UnityEngine.Vector3(packet.PosePosition.x, packet.PosePosition.y, packet.PosePosition.z);
                pab.previewPose.rotation = new UnityEngine.Quaternion(packet.PoseRotation.x, packet.PoseRotation.y, packet.PoseRotation.z, packet.PoseRotation.w);

                //Check if some mandatory variables are missing
                if (planet.physics == null || planet.physics.colChunks == null)
                {
                    planet.physics = new PlanetPhysics(planet);
                    planet.physics.Init();
                }
                if (AccessTools.Field(typeof(CargoTraffic), "beltRenderingBatch").GetValue(planet.factory.cargoTraffic) == null)
                {
                    planet.factory.cargoTraffic.CreateRenderingBatches();
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
                FactoryManager.IgnoreBasicBuildConditionChecks = true;
                bool canBuild = pab.CheckBuildConditions();
                canBuild &= CheckBuildingConnections(pab.buildPreviews, planet.factory);
                FactoryManager.IgnoreBasicBuildConditionChecks = false;

                if (canBuild)
                {
                    FactoryManager.PacketAuthor = packet.AuthorId;
                    pab.CreatePrebuilds();
                    FactoryManager.PacketAuthor = -1;
                }

                //Revert changes back to the original planet
                if (loadExternalPlanetData)
                {
                    planet.physics.Free();
                    planet.physics = null;
                    AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, tmpData, null);
                    GameMain.mainPlayer.mecha.buildArea = tmpBuildArea;
                    AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpPlanetPhysics);
                    AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpFactory);
                    AccessTools.Field(typeof(PlayerAction_Build), "nearcdLogic").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpNearcdLogic);
                }

                FactoryManager.EventFromServer = false;
                FactoryManager.EventFactory = null;
                pab.buildPreviews = tmpList;
                pab.waitConfirm = tmpConfirm;
                pab.previewPose.position = tmpPos; 
                pab.previewPose.rotation = tmpRot;
            }
        }

        public bool CheckBuildingConnections(List<BuildPreview> buildPreviews, PlanetFactory factory)
        {
            //Check if some entity that is suppose to be connected to this building is missing
            for(int i = 0; i < buildPreviews.Count; i++)
            {
                bool isInputOk = true;
                if (buildPreviews[i].inputObjId > 0)
                {
                    isInputOk = factory.entityPool.Length >= buildPreviews[i].inputObjId && factory.entityPool[buildPreviews[i].inputObjId].id != 0;
                } else if (buildPreviews[i].inputObjId < 0)
                {
                    isInputOk = factory.prebuildPool.Length >= -buildPreviews[i].inputObjId && factory.prebuildPool[-buildPreviews[i].inputObjId].id != 0;
                }
                bool isOutputOk = true;
                if (buildPreviews[i].outputObjId > 0)
                {
                    isInputOk = factory.entityPool.Length >= buildPreviews[i].outputObjId && factory.entityPool[buildPreviews[i].outputObjId].id != 0;
                }
                else if (buildPreviews[i].outputObjId < 0)
                {
                    isInputOk = factory.prebuildPool.Length >= -buildPreviews[i].outputObjId && factory.prebuildPool[-buildPreviews[i].outputObjId].id != 0;
                }
                if (!isInputOk || !isOutputOk)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
