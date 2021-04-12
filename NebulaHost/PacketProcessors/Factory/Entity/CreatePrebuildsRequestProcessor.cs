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
                PlanetFactory tmpFactory = (PlanetFactory)AccessTools.Field(typeof(PlayerAction_Build), "factory").GetValue(GameMain.mainPlayer.controller.actionBuild); 

                //Create Prebuilds from incomming packet
                pab.buildPreviews = packet.GetBuildPreviews();
                pab.waitConfirm = true;
                FactoryManager.EventFromServer = true;
                FactoryManager.EventFactory = planet.factory;
                pab.previewPose.position = new UnityEngine.Vector3(packet.PosePosition.x, packet.PosePosition.y, packet.PosePosition.z);
                pab.previewPose.rotation = new UnityEngine.Quaternion(packet.PoseRotation.x, packet.PoseRotation.y, packet.PoseRotation.z, packet.PoseRotation.w);
                AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.factory);
                if (planet.physics == null)
                {
                    planet.physics = new PlanetPhysics(planet);
                    planet.physics.Init();
                }
                if (AccessTools.Field(typeof(CargoTraffic), "beltRenderingBatch").GetValue(planet.factory.cargoTraffic) == null)
                {
                    planet.factory.cargoTraffic.CreateRenderingBatches();
                }
                pab.CreatePrebuilds();
                FactoryManager.EventFromServer = false;
                FactoryManager.EventFactory = null;

                //Revert changes back
                AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpFactory);
                pab.buildPreviews = tmpList;
                pab.waitConfirm = tmpConfirm;
                pab.previewPose.position = tmpPos;
                pab.previewPose.rotation = tmpRot;
            }
        }
    }
}
