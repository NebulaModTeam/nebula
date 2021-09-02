using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GameStatData))]
    class GameStatData_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameStatData.AfterTick))]
        public static void AfterTick_Postfix()
        {
            if (Multiplayer.IsActive && Multiplayer.Session.LocalPlayer.IsHost)
            {
                Multiplayer.Session.Statistics.CaptureStatisticalSnapshot();
            }
        }
    }
}
