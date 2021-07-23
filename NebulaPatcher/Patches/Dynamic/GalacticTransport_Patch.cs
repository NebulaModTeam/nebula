using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GalacticTransport.SetForNewGame))]
        public static void SetForNewGame_Postfix()
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new ILSRequestgStationPoolSync());
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GalacticTransport.RemoveStationComponent))]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || ILSShipManager.PatchLockILS;
        }
    }
}
