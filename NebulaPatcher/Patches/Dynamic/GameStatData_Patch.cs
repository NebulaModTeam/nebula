using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Statistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameStatData))]
    class GameStatData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("AfterTick")]
        public static void AfterTick_Postfix()
        {
            if (SimulatedWorld.Initialized && LocalPlayer.IsMasterClient)
            {
                StatisticsManager.instance.CaptureStatisticalSnapshot();
            }
        }
    }
}
