using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using System;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSphere))]
    internal class DysonSphere_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSphere.BeforeGameTick))]
        public static bool BeforeGameTick_Prefix(DysonSphere __instance, long times)
        {
            if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
            {
                return true;
            }
            //Update swarm and layer energy generation stats every 120 frames
            if (times % 120 == 0)
            {
                __instance.swarm.energyGenCurrentTick = __instance.swarm.sailCount * __instance.energyGenPerSail;
                for (int i = 0; i < 10; i++)
                {
                    DysonSphereLayer dysonSphereLayer = __instance.layersSorted[i];
                    if (dysonSphereLayer != null)
                    {
                        dysonSphereLayer.energyGenCurrentTick = 0L;
                        DysonNode[] nodePool = dysonSphereLayer.nodePool;
                        DysonShell[] shellPool = dysonSphereLayer.shellPool;
                        for (int j = 1; j < dysonSphereLayer.nodeCursor; j++)
                        {
                            if (nodePool[j] != null && nodePool[j].id == j)
                            {
                                dysonSphereLayer.energyGenCurrentTick += nodePool[j].EnergyGenCurrentTick(__instance.energyGenPerNode, __instance.energyGenPerFrame, 0L);
                            }
                        }
                        for (int k = 1; k < dysonSphereLayer.shellCursor; k++)
                        {
                            if (shellPool[k] != null && shellPool[k].id == k)
                            {
                                dysonSphereLayer.energyGenCurrentTick += shellPool[k].cellPoint * __instance.energyGenPerShell;
                            }
                        }
                    }
                }
            }
            //Sync other Dyson sphere status related to ray receivers on client side by DysonSphereStatusPacket
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSphere.AddLayer))]
        public static void AddLayer_Postfix(DysonSphere __instance, float orbitRadius, Quaternion orbitRotation, float orbitAngularSpeed)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
            //If local is the author and not in the process of importing blueprint
            if (!Multiplayer.Session.DysonSpheres.IsIncomingRequest && !Multiplayer.Session.DysonSpheres.InBlueprint)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSphereAddLayerPacket(__instance.starData.index, __instance.QueryLayerId(), orbitRadius, orbitRotation, orbitAngularSpeed));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSphere.RemoveLayer), new Type[] { typeof(int) })]
        public static void RemoveLayer_Prefix(DysonSphere __instance, int id)
        {
            if (Multiplayer.IsActive)
            {
                RemoveLayer(id, __instance.starData.index);
            }            
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSphere.RemoveLayer), new Type[] { typeof(DysonSphereLayer) })]
        public static void RemoveLayer_Prefix2(DysonSphere __instance, DysonSphereLayer layer)
        {
            if (Multiplayer.IsActive)
            {
                RemoveLayer(layer.id, __instance.starData.index);
            }
        }

        public static void RemoveLayer(int id, int starIndex)
        {
            //If local is the author and not in the process of importing blueprint
            if (!Multiplayer.Session.DysonSpheres.IsIncomingRequest && !Multiplayer.Session.DysonSpheres.InBlueprint)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSphereRemoveLayerPacket(starIndex, id));
            }
        }
    }
}
