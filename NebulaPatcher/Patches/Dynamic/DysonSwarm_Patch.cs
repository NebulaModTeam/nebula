using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using NebulaWorld.Universe;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSwarm))]
    class DysonSwarm_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("NewOrbit")]
        public static bool Prefix1(DysonSwarm __instance, int __result, float radius, Quaternion rotation)
        {
            //Notify others that orbit for Dyson Swarm was created
            if (!DysonSphere_Manager.IncomingDysonSwarmPacket)
            {
                LocalPlayer.SendPacket(new DysonSwarmAddOrbitPacket(__instance.starData.index, radius, rotation));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveOrbit")]
        public static bool Prefix2(DysonSwarm __instance, int orbitId)
        {
            //Notify others that orbit for Dyson Swarm was deleted
            if (!DysonSphere_Manager.IncomingDysonSwarmPacket)
            {
                LocalPlayer.SendPacket(new DysonSwarmRemoveOrbitPacket(__instance.starData.index, orbitId));
            }
            return true;
        }
    }
}
