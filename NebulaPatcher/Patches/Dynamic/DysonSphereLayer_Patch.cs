using HarmonyLib;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Universe;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSphereLayer))]
    class DysonSphereLayer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("NewDysonNode")]
        public static bool Prefix1(DysonSphereLayer __instance, int __result, int protoId, Vector3 pos)
        {
            //Notify others that user added node to the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereAddNodePacket(__instance.starData.index, __instance.id, protoId, new Float3(pos)));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("NewDysonFrame")]
        public static bool Prefix2(DysonSphereLayer __instance, int __result, int protoId, int nodeAId, int nodeBId, bool euler)
        {
            //Notify others that user added frame to the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereAddFramePacket(__instance.starData.index, __instance.id, protoId, nodeAId, nodeBId, euler));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveDysonFrame")]
        public static bool Prefix3(DysonSphereLayer __instance, int frameId)
        {
            //Notify others that user removed frame from the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereRemoveFramePacket(__instance.starData.index, __instance.id, frameId));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveDysonNode")]
        public static bool Prefix4(DysonSphereLayer __instance, int nodeId)
        {
            //Notify others that user removed node from the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereRemoveNodePacket(__instance.starData.index, __instance.id, nodeId));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("NewDysonShell")]
        public static bool Prefix5(DysonSphereLayer __instance, int protoId, List<int> nodeIds)
        {
            //Notify others that user removed node from the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereAddShellPacket(__instance.starData.index, __instance.id, protoId, nodeIds));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveDysonShell")]
        public static bool Prefix6(DysonSphereLayer __instance, int shellId)
        {
            //Notify others that user removed node from the dyson plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereRemoveShellPacket(__instance.starData.index, __instance.id, shellId));
            }
            return true;
        }
    }
}
