using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Universe;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSphere))]
    class DysonSphere_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddLayer")]
        public static bool AddLayer_Prefix(DysonSphere __instance, DysonSphereLayer __result, float orbitRadius, Quaternion orbitRotation, float orbitAngularSpeed)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }
            //Notify others that user added layer to dyson sphere plan
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereAddLayerPacket(__instance.starData.index, orbitRadius, orbitRotation, orbitAngularSpeed));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveLayer", new Type[] { typeof(int) })]
        public static bool RemoveLayer_Prefix(DysonSphere __instance, int id)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }
            //Notify others that user removed layer to dyson sphere plan
            RemoveLayer(id, __instance.starData.index);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveLayer", new Type[] { typeof(DysonSphereLayer) })]
        public static bool RemoveLayer_Prefix2(DysonSphere __instance, DysonSphereLayer layer)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }
            //Notify others that user removed layer to dyson sphere plan
            RemoveLayer(layer.id, __instance.starData.index);
            return true;
        }

        public static void RemoveLayer(int id, int starIndex)
        {
            if (!DysonSphere_Manager.IncomingDysonSpherePacket)
            {
                LocalPlayer.SendPacket(new DysonSphereRemoveLayerPacket(starIndex, id));
            }
        }
    }
}
