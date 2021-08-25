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
        [HarmonyPatch(nameof(DysonSwarm.NewOrbit))]
        public static bool NewOrbit_Prefix(DysonSwarm __instance, int __result, float radius, Quaternion rotation)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }
            //Notify others that orbit for Dyson Swarm was created
            if (!Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket)
            {
                LocalPlayer.SendPacket(new DysonSwarmAddOrbitPacket(__instance.starData.index, radius, rotation));
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSwarm.RemoveOrbit))]
        public static bool RemoveOrbit_Prefix(DysonSwarm __instance, int orbitId)
        {
            if (!Multiplayer.IsActive)
            {
                return true;
            }
            //Notify others that orbit for Dyson Swarm was deleted
            if (!Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket)
            {
                LocalPlayer.SendPacket(new DysonSwarmRemoveOrbitPacket(__instance.starData.index, orbitId));
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(DysonSwarm.AddBullet))]
        public static void AddBullet_Postfix(DysonSwarm __instance, SailBullet bullet, int orbitId)
        {
            //Host is sending correction / authorization packet to correct constants of the generated bullet
            if (Multiplayer.IsActive && LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new DysonSphereBulletCorrectionPacket(__instance.starData.index, bullet.id, bullet.uEndVel, bullet.uEnd));
            }
        }
    }
}
