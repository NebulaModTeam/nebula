using HarmonyLib;
using NebulaModel.Logger;
using NebulaWorld;
using NebulaModel.Packets.Logistics;
using NebulaWorld.Logistics;
using System.Collections.Generic;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetForNewGame")]
        public static void SetForNewGame_Postfix()
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new ILSRequestgStationPoolSync());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("AddStationComponent")]
        public static bool AddStationComponent_Prefix(GalacticTransport __instance, int planetId, StationComponent station)
        {
            // Method only allowed to run if we're host or it's while processing a packet from host.
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || ILSShipManager.PatchLockILS;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveStationComponent")]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || ILSShipManager.PatchLockILS;
        }
    }
}
