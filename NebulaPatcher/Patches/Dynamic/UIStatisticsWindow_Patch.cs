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
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Postfix()
        {
            if (Multiplayer.IsActive && !LocalPlayer.IsMasterClient)
            {
                Multiplayer.Session.Statistics.IsStatisticsNeeded = true;
                LocalPlayer.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowOpened));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void _OnClose_Postfix()
        {
            if (Multiplayer.IsActive && !LocalPlayer.IsMasterClient && Multiplayer.Session.Statistics.IsStatisticsNeeded)
            {
                Multiplayer.Session.Statistics.IsStatisticsNeeded = false;
                LocalPlayer.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowClosed));
            }
        }
    }
}
