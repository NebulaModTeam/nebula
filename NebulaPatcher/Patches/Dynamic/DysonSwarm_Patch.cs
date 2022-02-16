using HarmonyLib;
using NebulaModel.Packets.Universe;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSwarm))]
    internal class DysonSwarm_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSwarm.NewOrbit))]
        public static void NewOrbit_Prefix(DysonSwarm __instance, float radius, Quaternion rotation)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
            //If local is the author and not in the process of importing blueprint
            if (!Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket && !Multiplayer.Session.DysonSpheres.InBlueprint)
            {
                int orbitId = NebulaWorld.Universe.DysonSphereManager.QueryOrbitId(__instance);
                Multiplayer.Session.Network.SendPacket(new DysonSwarmAddOrbitPacket(__instance.starData.index, orbitId, radius, rotation));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSwarm.RemoveOrbit))]
        public static void RemoveOrbit_Prefix(DysonSwarm __instance, int orbitId)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
            //If local is the author and not in the process of importing blueprint
            if (!Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket && !Multiplayer.Session.DysonSpheres.InBlueprint)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSwarmRemoveOrbitPacket(__instance.starData.index, orbitId, SwarmRemoveOrbitEvent.Remove));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSwarm.SetOrbitEnable))]
        public static void SetOrbitEnable_Prefix(DysonSwarm __instance, int orbitId, bool enabled)
        {
            if (!Multiplayer.IsActive)
            {
                return;
            }
            //Notify others that orbit for Dyson Swarm was enabled/disabled
            if (!Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSwarmRemoveOrbitPacket(__instance.starData.index, orbitId, enabled ? SwarmRemoveOrbitEvent.Enable : SwarmRemoveOrbitEvent.Disable));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DysonSwarm.EditOrbit))]
        public static void EditOrbit_Prefix(DysonSwarm __instance, int orbitId, float radius, Quaternion rotation)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.DysonSpheres.IncomingDysonSwarmPacket)
            {
                Multiplayer.Session.Network.SendPacket(new DysonSwarmEditOrbitPacket(__instance.starData.index, orbitId, radius, rotation));
            }
        }
    }
}
