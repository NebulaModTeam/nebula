using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : IPacketProcessor<CreatePrebuildsRequest>
    {
        public void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            if (pab != null)
            {
                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = pab.buildPreviews;
                bool tmpConfirm = pab.waitConfirm;
                UnityEngine.Vector3 tmpPos = pab.previewPose.position;
                UnityEngine.Quaternion tmpRot = pab.previewPose.rotation;

                //Create Prebuilds from incomming packet
                pab.buildPreviews = packet.GetBuildPreviews();
                pab.waitConfirm = true;
                FactoryManager.EventFromServer = true;
                pab.previewPose.position = new UnityEngine.Vector3(packet.PosePosition.x, packet.PosePosition.y, packet.PosePosition.z);
                pab.previewPose.rotation = new UnityEngine.Quaternion(packet.PoseRotation.x, packet.PoseRotation.y, packet.PoseRotation.z, packet.PoseRotation.w);
                AccessTools.Field(typeof(PlayerAction_Build), "factory").SetValue(GameMain.mainPlayer.controller.actionBuild, GameMain.localPlanet.factory);
                pab.CreatePrebuilds();
                FactoryManager.EventFromServer = false;

                //Revert changes back
                pab.buildPreviews = tmpList;
                pab.waitConfirm = tmpConfirm;
                pab.previewPose.position = tmpPos;
                pab.previewPose.rotation = tmpRot;
            }
        }
    }
}
