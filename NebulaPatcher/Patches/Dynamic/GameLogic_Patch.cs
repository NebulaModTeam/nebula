#region

using System;
using HarmonyLib;
using NebulaModel.Logger;
using NebulaPatcher.Patches.Transpilers;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GameLogic))]
public class GameLogic_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameLogic.Draw))]
    public static void Draw_Postfix()
    {
        if (!Multiplayer.IsActive || !Multiplayer.Session.IsGameLoaded)
        {
            return;
        }

        Multiplayer.Session.World.RenderPlayerNameTagsInGame();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameLogic.LogicFrame))]
    public static void LogicFrame_Postfix(bool __runOriginal)
    {
        if (!Multiplayer.IsActive || !__runOriginal)
        {
            return;
        }
        var time = GameMain.gameTick;
        Multiplayer.Session.Couriers.GameTick();
        Multiplayer.Session.Belts.GameTick();
        Multiplayer.Session.Combat.GameTick();

        if (Multiplayer.Session.IsServer)
        {
            Multiplayer.Session.Launch.CollectProjectile();
            Multiplayer.Session.Statistics.SendBroadcastIfNeeded(time);
            return;
        }

        try
        {
            // Client: Update visual effects that don't affect the production
            Multiplayer.Session.Launch.LaunchProjectile();
            ILSUpdateShipPos(time);
        }
        catch (Exception e)
        {
            _ = e;
#if DEBUG
            Log.Warn(e);
#endif
        }
    }

    private static void ILSUpdateShipPos(long time)
    {
        if (!Multiplayer.Session.IsGameLoaded) return;

        // call StationComponent::InternalTickRemote() from here, see StationComponent_Patch.cs for info
        var timeGene = (int)(time % 60L);
        if (timeGene < 0)
        {
            timeGene += 60;
        }
        var history = GameMain.history;
        var shipSailSpeed = history.logisticShipSailSpeedModified;
        var shipWarpSpeed = !history.logisticShipWarpDrive ? shipSailSpeed : history.logisticShipWarpSpeedModified;
        var shipCarries = history.logisticShipCarries;
        var gameData = GameMain.data;
        var gStationPool = gameData.galacticTransport.stationPool;
        var astroPoses = gameData.galaxy.astrosData;
        var relativePos = gameData.relativePos;
        var relativeRot = gameData.relativeRot;
        var starmap = UIGame.viewMode == EViewMode.Starmap;

        foreach (var stationComponent in GameMain.data.galacticTransport.stationPool)
        {
            if (stationComponent != null && stationComponent.isStellar && stationComponent.planetId > 0)
            {
                var planet = GameMain.galaxy.PlanetById(stationComponent.planetId);
                if (planet == null) continue;

                StationComponent_Transpiler.ILSUpdateShipPos(stationComponent,
                    planet.factory, timeGene, shipSailSpeed, shipWarpSpeed,
                    shipCarries, gStationPool, astroPoses, ref relativePos, ref relativeRot, starmap, null);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameLogic.OnFactoryFrameBegin))]
    public static void OnFactoryFrameBegin_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameLogic.OnFactoryFrameEnd))]
    public static void OnFactoryFrameEnd_Postfix()
    {
        if (Multiplayer.IsActive)
        {
            Multiplayer.Session.Storage.IsHumanInput = true;
        }
    }
}
