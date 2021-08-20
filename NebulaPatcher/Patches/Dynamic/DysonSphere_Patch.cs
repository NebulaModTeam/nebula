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
        [HarmonyPatch(nameof(DysonSphere.AddLayer))]
        public static bool AddLayer_Prefix(DysonSphere __instance, float orbitRadius, Quaternion orbitRotation, float orbitAngularSpeed)
        {
            if (!SimulatedWorld.Initialized)
            {
                return true;
            }
            //Notify others that user added layer to dyson sphere plan
            if (!DysonSphereManager.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new DysonSphereAddLayerPacket(__instance.starData.index, orbitRadius, orbitRotation, orbitAngularSpeed));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSphere.RemoveLayer), new Type[] { typeof(int) })]
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
        [HarmonyPatch(nameof(DysonSphere.RemoveLayer), new Type[] { typeof(DysonSphereLayer) })]
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
            if (!DysonSphereManager.IsIncomingRequest)
            {
                LocalPlayer.SendPacket(new DysonSphereRemoveLayerPacket(starIndex, id));
            }
        }
    }
}
