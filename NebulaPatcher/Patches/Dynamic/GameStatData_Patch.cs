using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Statistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameStatData))]
    class GameStatData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameStatData.AfterTick))]
        public static void AfterTick_Postfix()
        {
            if (Multiplayer.IsActive && ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
            {
                Multiplayer.Session.Statistics.CaptureStatisticalSnapshot();
            }
        }
    }
}
