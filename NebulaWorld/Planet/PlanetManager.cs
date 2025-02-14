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

    public void Dispose()
    {
        PendingFactories = null;
        PendingTerrainData = null;
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
        // Temporarily clear all CustomCharts on the unloaded factories to avoid errors
        gameData.statistics.charts.Free();
        gameData.statistics.charts.Init(gameData);
    }
}
