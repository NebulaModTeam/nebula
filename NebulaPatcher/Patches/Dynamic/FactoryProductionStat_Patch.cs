using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Statistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(FactoryProductionStat))]
    class FactoryProductionStat_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FactoryProductionStat.GameTick))]
        public static bool GameTick_Prefix(FactoryProductionStat __instance)
        {
            //Do not run in single player for host
            if (!SimulatedWorld.Initialized || LocalPlayer.Instance.IsMasterClient)
            {
                return true;
            }

            //Multiplayer clients should not include their own calculated statistics
            if (!StatisticsManager.IsIncomingRequest)
            {
                __instance.ClearRegisters();
                return false;
            }

            return true;
        }
    }
}
