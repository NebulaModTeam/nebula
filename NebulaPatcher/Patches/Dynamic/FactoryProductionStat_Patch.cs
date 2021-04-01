using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Statistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactoryProductionStat))]
    class FactoryProductionStat_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GameTick")]
        public static bool GameTick_Prefix(FactoryProductionStat __instance)
        {
            //Do not calculate statistics in the multiplayer for the clients if it is not triggered by the server
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }

            //Multiplayer clients should not include their own calculated statistics
            if (!StatisticsManager.IsIncommingRequest)
            {
                __instance.ClearRegisters();
                return false;
            }

            return true;
        }
    }
}
