#region

using System;
using System.Collections.Generic;
using NebulaModel.DataStructures;
using NebulaModel.Logger;

#endregion

namespace NebulaWorld.Planet;

public class PlanetManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();
    public int TargetPlanet { get; set; }

    public Dictionary<int, byte[]> PendingFactories { get; set; } = new();
    public Dictionary<int, byte[]> PendingTerrainData { get; set; } = new();
    public bool EnableVeinPacket { get; set; } = true;
    public static byte[] PreservedDashboardData { get; set; }

    public void Dispose()
    {
        PendingFactories = null;
        PendingTerrainData = null;
        if (!GameStates.GameStatesManager.DuringReconnect)
        {
            PreservedDashboardData = null;
        }
        GC.SuppressFinalize(this);
    }

    public static void UnloadAllFactories()
    {
        Log.Info("UnloadAllFactories");
        var gameData = GameMain.data;
        Multiplayer.Session.Drones.ClearAllRemoteDrones();
        using (Multiplayer.Session.Ships.PatchLockILS.On())
        {
            for (var i = gameData.factoryCount - 1; i >= 0; i--)
            {
                var planet = gameData.factories[i].planet;
                planet.factory.Free();
                planet.factory = null;
                gameData.galaxy.astrosFactory[planet.id] = null;  //Assigned by UpdateRuntimePose
            }
            gameData.factoryCount = 0;
            Multiplayer.Session.Combat.OnAstroFactoryUnload();
        }

        // Ensure active dashboard state is cleanly collected and preserved before charts.Free() and charts.Init()
        var dashboard = UIRoot.instance?.uiGame?.dashboard;
        if (dashboard != null && dashboard.active)
        {
            try
            {
                dashboard.CollectStates();
                dashboard._Close();
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to collect dashboard states before factory unload: {e}");
            }
        }

        // Temporarily clear all CustomCharts on the unloaded factories to avoid errors, but preserve layout
        if (gameData.statistics?.charts?.statPlans != null && gameData.statistics.charts.statPlans.count > 0)
        {
            try
            {
                using var ms = new System.IO.MemoryStream();
                using (var writer = new System.IO.BinaryWriter(ms))
                {
                    gameData.statistics.charts.Export(writer);
                }
                PreservedDashboardData = ms.ToArray();
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to snapshot charts before factory unload: {e}");
            }
        }
        gameData.statistics?.charts?.Free();
        gameData.statistics?.charts?.Init(gameData);
    }
}
