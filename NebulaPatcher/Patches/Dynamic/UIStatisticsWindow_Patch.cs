#region

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Statistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStatisticsWindow))]
internal class UIStatisticsWindow_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStatisticsWindow._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        Multiplayer.Session.Statistics.IsStatisticsNeeded = true;
        Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowOpened));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStatisticsWindow._OnClose))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnClose_Postfix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost ||
            !Multiplayer.Session.Statistics.IsStatisticsNeeded)
        {
            return;
        }
        Multiplayer.Session.Statistics.IsStatisticsNeeded = false;
        Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowClosed));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStatisticsWindow.AddFactoryStatGroup))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool AddFactoryStatGroup_Prefix(int _factoryIndex, ProductionStatistics ___productionStat)
    {
        //Skip when StatisticsDataPacket hasn't arrived yet
        return _factoryIndex >= 0 && ___productionStat.factoryStatPool[_factoryIndex] != null;
    }
}
