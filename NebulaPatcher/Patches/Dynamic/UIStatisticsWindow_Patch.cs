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
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStatisticsWindow._OnOpen))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnOpen_Postfix(UIStatisticsWindow __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        Multiplayer.Session.Statistics.IsStatisticsNeeded = true;
        var astroFilter = __instance.astroFilter;
        if (astroFilter == 0)
        {
            astroFilter = GameMain.localPlanet?.astroId ?? (GameMain.localStar?.id ?? 0);
        }
        Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowOpened, astroFilter));
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
        Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.WindowClosed, 0));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStatisticsWindow.AddProductStatGroup))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static bool AddProductStatGroup_Prefix(int _factoryIndex, ProductionStatistics ___productionStat)
    {
        //Skip when StatisticsDataPacket hasn't arrived yet
        return _factoryIndex >= 0 && ___productionStat.factoryStatPool[_factoryIndex] != null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStatisticsWindow.AstroBoxToValue))]
    public static void AstroBoxToValue_Postfix(UIStatisticsWindow __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost) return;

        if (__instance.isStatisticsTab && __instance.lastAstroFilter != __instance.astroFilter)
        {
            if (__instance.astroFilter != 0)
            {
                Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.AstroFilterChanged, __instance.astroFilter));
            }
            else if (GameMain.localPlanet == null && GameMain.localStar != null) // local star
            {
                Multiplayer.Session.Network.SendPacket(new StatisticsRequestEvent(StatisticEvent.AstroFilterChanged, GameMain.localStar.astroId));
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStatisticsWindow.RefreshProductionExtraInfo))]
    public static bool Refresh_Prefix(UIStatisticsWindow __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost) return true;

        // Client: Only allow refresh on local planet. Otherwise use request to get data from server
        return GameMain.localPlanet != null && (__instance.astroFilter == 0 || __instance.astroFilter == GameMain.data.localPlanet.id);
    }
}
