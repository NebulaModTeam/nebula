#region

using System;
using System.IO;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaWorld;
using NebulaWorld.Planet;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch]
internal class UIDashboard_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIDashboard), nameof(UIDashboard._OnOpen))]
    public static void UIDashboard_OnOpen_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        if (GameMain.data?.statistics?.charts != null &&
            (GameMain.data.statistics.charts.statPlans == null || GameMain.data.statistics.charts.statPlans.count == 0) &&
            PlanetManager.PreservedDashboardData != null && PlanetManager.PreservedDashboardData.Length > 0)
        {
            try
            {
                using var ms = new MemoryStream(PlanetManager.PreservedDashboardData);
                using var reader = new BinaryReader(ms);
                GameMain.data.statistics.charts.Import(reader);
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to restore dashboard data on open: {e}");
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIDashboard), nameof(UIDashboard._OnClose))]
    public static void UIDashboard_OnClose_Postfix(UIDashboard __instance)
    {
        try
        {
            __instance.CollectStates();
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to collect dashboard states: {e}");
        }
        SyncDashboardToServer();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CustomCharts), nameof(CustomCharts.CreateOrFindStatPlan))]
    public static void CustomCharts_CreateOrFindStatPlan_Postfix()
    {
        SyncDashboardToServer();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CustomCharts), nameof(CustomCharts.RemoveStatPlan))]
    public static void CustomCharts_RemoveStatPlan_Postfix()
    {
        SyncDashboardToServer();
    }

    public static void SyncDashboardToServer()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        if (GameMain.data?.statistics?.charts == null)
        {
            return;
        }

        try
        {
            var dashboard = UIRoot.instance?.uiGame?.dashboard;
            if (dashboard != null && dashboard.active)
            {
                dashboard.CollectStates();
            }

            var charts = GameMain.data.statistics.charts;
            if ((charts.statPlans == null || charts.statPlans.count == 0) &&
                PlanetManager.PreservedDashboardData != null && PlanetManager.PreservedDashboardData.Length > 0)
            {
                return;
            }

            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {
                charts.Export(writer);
            }
            var data = ms.ToArray();
            PlanetManager.PreservedDashboardData = data;
            Multiplayer.Session.Network.SendPacket(new PlayerDashboardPacket(Multiplayer.Session.LocalPlayer.Id, data));
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to sync dashboard to server: {e}");
        }
    }
}
