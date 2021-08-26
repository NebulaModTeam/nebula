using HarmonyLib;
using NebulaModel.Packets.Statistics;
using NebulaWorld;
using NebulaWorld.Statistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStatisticsWindow))]
    class UIStatisticsWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStatisticsWindow._OnOpen))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnOpen_Postfix()
        {
            if (SimulatedWorld.Instance.Initialized && !LocalPlayer.Instance.IsMasterClient)
            {
                StatisticsManager.IsStatisticsNeeded = true;
                LocalPlayer.Instance.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowOpened));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStatisticsWindow._OnClose))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnClose_Postfix()
        {
            if (SimulatedWorld.Instance.Initialized && !LocalPlayer.Instance.IsMasterClient && StatisticsManager.IsStatisticsNeeded)
            {
                StatisticsManager.IsStatisticsNeeded = false;
                LocalPlayer.Instance.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowClosed));
            }
        }
    }
}
