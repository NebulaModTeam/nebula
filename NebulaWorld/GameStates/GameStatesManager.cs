#region

using System;
using System.Collections.Generic;
using NebulaModel;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using UnityEngine;

#endregion

namespace NebulaWorld.GameStates;

public class GameStatesManager : IDisposable
{
    public const float MaxUPS = 240f;
    public const float MinUPS = 30f;
    public static bool DuringReconnect { get; set; }
    private static readonly HashSet<int> preservedFeatureKeys = new();
    private static readonly HashSet<int> preservedTutorialUnlocked = new();
    private static int bufferLength;

    public static bool IsAdvisorOrTutorialFeatureKey(int featureId)
    {
        return (featureId >= FeatureID.ADVISOR_TIP_START && featureId < FeatureID.GOAL_STATE) ||
               (featureId >= FeatureID.VEIN_SCAN && featureId <= FeatureID.HIDE_GRID_SPLIT_TIP);
    }

    public static void PreserveFeatureKey(int featureId)
    {
        if (IsAdvisorOrTutorialFeatureKey(featureId))
        {
            preservedFeatureKeys.Add(featureId);
        }
    }

    public static void UnpreserveFeatureKey(int featureId)
    {
        if (IsAdvisorOrTutorialFeatureKey(featureId))
        {
            preservedFeatureKeys.Remove(featureId);
        }
    }

    public static void PreserveTutorial(int tutorialId)
    {
        if (tutorialId > 0)
        {
            preservedTutorialUnlocked.Add(tutorialId);
        }
    }

    public static byte[] PreservedReplicatorMultipliers { get; set; }

    public static void ApplyReplicatorMultipliers(byte[] data, UIReplicatorWindow replicatorTarget = null)
    {
        if (data == null || data.Length < 4)
        {
            return;
        }

        try
        {
            using var ms = new System.IO.MemoryStream(data);
            using var reader = new System.IO.BinaryReader(ms);
            int count = reader.ReadInt32();
            if (count < 0 || count > 50000 || data.Length < 4 + count * 8)
            {
                return;
            }

            PreservedReplicatorMultipliers = data;

            var gameData = GameMain.data;
            if (gameData?.preferences != null)
            {
                gameData.preferences.replicatorMultipliers ??= new Dictionary<int, int>();
            }

            var replicator = replicatorTarget ?? UIRoot.instance?.uiGame?.replicator;
            if (replicator != null)
            {
                replicator.multipliers ??= new Dictionary<int, int>();
            }

            for (int i = 0; i < count; i++)
            {
                int recipeId = reader.ReadInt32();
                int multi = reader.ReadInt32();
                if (gameData?.preferences?.replicatorMultipliers != null)
                {
                    gameData.preferences.replicatorMultipliers[recipeId] = multi;
                }
                if (replicator?.multipliers != null)
                {
                    replicator.multipliers[recipeId] = multi;
                }
            }

            if (replicator != null && replicator.selectedRecipe != null && replicator.multiValueText != null)
            {
                int currentMulti = 1;
                if (replicator.multipliers != null && replicator.multipliers.TryGetValue(replicator.selectedRecipe.ID, out int val) && val > 1)
                {
                    currentMulti = val;
                }
                replicator.multiValueText.text = $"{currentMulti}x";
            }
        }
        catch (Exception e)
        {
            Log.Warn($"Failed to apply replicator multipliers: {e}");
        }
    }

    public static byte[] ExportReplicatorMultipliers()
    {
        Dictionary<int, int> source = null;

        var replicator = UIRoot.instance?.uiGame?.replicator;
        if (replicator?.multipliers != null && replicator.multipliers.Count > 0)
        {
            source = replicator.multipliers;
        }
        else if (GameMain.data?.preferences?.replicatorMultipliers != null && GameMain.data.preferences.replicatorMultipliers.Count > 0)
        {
            source = GameMain.data.preferences.replicatorMultipliers;
        }

        if (source != null && source.Count > 0)
        {
            try
            {
                using var ms = new System.IO.MemoryStream();
                using var writer = new System.IO.BinaryWriter(ms);
                writer.Write(source.Count);
                foreach (var kv in source)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value);
                }
                var bytes = ms.ToArray();
                PreservedReplicatorMultipliers = bytes;
                return bytes;
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to export replicator multipliers: {e}");
            }
        }

        return PreservedReplicatorMultipliers ?? Array.Empty<byte>();
    }

    public static void RestoreReplicatorMultipliers(UIReplicatorWindow replicator)
    {
        if (replicator == null)
        {
            return;
        }
        replicator.multipliers ??= new Dictionary<int, int>();

        if (PreservedReplicatorMultipliers != null && PreservedReplicatorMultipliers.Length > 0)
        {
            ApplyReplicatorMultipliers(PreservedReplicatorMultipliers, replicator);
            return;
        }

        var prefMultipliers = GameMain.data?.preferences?.replicatorMultipliers;
        if (prefMultipliers != null && prefMultipliers.Count > 0)
        {
            foreach (var kv in prefMultipliers)
            {
                replicator.multipliers[kv.Key] = kv.Value;
            }
            PreservedReplicatorMultipliers = ExportReplicatorMultipliers();
        }
        else if (replicator.multipliers.Count > 0)
        {
            if (GameMain.data?.preferences != null)
            {
                gameDataPreferencesSync(replicator);
            }
            PreservedReplicatorMultipliers = ExportReplicatorMultipliers();
        }

        if (replicator.selectedRecipe != null && replicator.multiValueText != null)
        {
            int currentMulti = 1;
            if (replicator.multipliers.TryGetValue(replicator.selectedRecipe.ID, out int val) && val > 1)
            {
                currentMulti = val;
            }
            replicator.multiValueText.text = $"{currentMulti}x";
        }
    }

    private static void gameDataPreferencesSync(UIReplicatorWindow replicator)
    {
        GameMain.data.preferences.replicatorMultipliers ??= new Dictionary<int, int>();
        foreach (var kv in replicator.multipliers)
        {
            GameMain.data.preferences.replicatorMultipliers[kv.Key] = kv.Value;
        }
    }

    public static byte[] ExportDashboardData()
    {
        var dashboard = UIRoot.instance?.uiGame?.dashboard;
        if (dashboard != null && dashboard.active)
        {
            try
            {
                dashboard.CollectStates();
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to collect dashboard states: {e}");
            }
        }

        var charts = GameMain.data?.statistics?.charts;
        if (charts?.statPlans != null && charts.statPlans.count > 0)
        {
            try
            {
                using var ms = new System.IO.MemoryStream();
                using var writer = new System.IO.BinaryWriter(ms);
                charts.Export(writer);
                var bytes = ms.ToArray();
                Planet.PlanetManager.PreservedDashboardData = bytes;
                return bytes;
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to export dashboard data: {e}");
            }
        }

        return Planet.PlanetManager.PreservedDashboardData;
    }

    public static long RealGameTick => GameMain.gameTick;
    public static float RealUPS => (float)FPSController.currentUPS;
    public static long LastSaveTime { get; set; } // UnixTimeSeconds
    public static int FragmentSize { get; set; }

    // UPS syncing by GameStateUpdate packet
    private readonly float BUFFERING_TICK = 60f;
    private readonly float BUFFERING_TIME = 30f;
    private float averageUPS = 60f;
    private int averageRTT;
    private bool hasChanged;

    // Store data get from GlobalGameDataResponse
    private bool sandboxToolsEnabled;
    private byte[] historyBinaryData;
    private byte[] galacticTransportBinaryData;
    private byte[] spaceSectorBinaryData;
    private byte[] milestoneSystemBinaryData;
    private byte[] trashSystemBinaryData;
    private byte[] galacticDigitalBinaryData;

    public GameStatesManager()
    {
        LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void Dispose()
    {
        if (!DuringReconnect)
        {
            preservedFeatureKeys.Clear();
            preservedTutorialUnlocked.Clear();
            PreservedReplicatorMultipliers = null;
        }

        LastSaveTime = FragmentSize = 0;
        sandboxToolsEnabled = false;
        historyBinaryData = null;
        galacticTransportBinaryData = null;
        spaceSectorBinaryData = null;
        milestoneSystemBinaryData = null;
        trashSystemBinaryData = null;
        galacticDigitalBinaryData = null;
        GC.SuppressFinalize(this);
    }

    public float GetServerUPS()
    {
        return averageUPS;
    }

    public void ProcessGameStateUpdatePacket(long sentTime, long gameTick, float unitsPerSecond)
    {
        var rtt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sentTime;
        averageRTT = (int)(averageRTT * 0.8 + rtt * 0.2);
        averageUPS = averageUPS * 0.8f + unitsPerSecond * 0.2f;
        Multiplayer.Session.World.UpdatePingIndicator($"Ping: {averageRTT}ms");

        // We offset the tick received to account for the time it took to receive the packet
        var tickOffsetSinceSent = (long)Math.Round(unitsPerSecond * rtt / 2 / 1000);
        var currentGameTick = gameTick + tickOffsetSinceSent;
        var diff = currentGameTick - GameMain.gameTick;

        // Discard abnormal packet (usually after host saving the file)
        if (rtt > 2 * averageRTT || averageUPS - unitsPerSecond > 15)
        {
            // Initial connection
            if (GameMain.gameTick < 1200L)
            {
                averageRTT = (int)rtt;
                GameMain.gameTick = currentGameTick;
            }
            Log.Debug(
                $"GameStateUpdate unstable. RTT:{rtt}(avg{averageRTT}) UPS:{unitsPerSecond:F2}(avg{averageUPS:F2})");
            return;
        }

        if (!Config.Options.SyncUps)
        {
            // We allow for a small drift of 5 ticks since the tick offset using the ping is only an approximation
            if (GameMain.gameTick > 0 && Mathf.Abs(diff) > 5)
            {
                Log.Debug($"Game Tick desync. {GameMain.gameTick} skip={diff} UPS:{unitsPerSecond:F2}(avg{averageUPS:F2})");
                GameMain.gameTick = currentGameTick;
            }
            // Reset FixUPS when user turns off the option
            if (!hasChanged)
            {
                return;
            }
            FPSController.SetFixUPS(0);
            hasChanged = false;
            return;
        }

        // Adjust client's UPS to match game tick with server, range 30~120 UPS
        var ups = diff / 1f + averageUPS;
        long skipTick = 0;
        switch (ups)
        {
            case > MaxUPS:
                {
                    // Try to distribute game tick difference into BUFFERING_TIME (seconds)
                    if (diff / BUFFERING_TIME + averageUPS > MaxUPS)
                    {
                        // The difference is too large, need to skip ticks to catch up
                        skipTick = (long)(ups - MaxUPS);
                    }
                    ups = MaxUPS;
                    break;
                }
            case < MinUPS:
                {
                    if (diff + averageUPS - MinUPS < -BUFFERING_TICK)
                    {
                        skipTick = (long)(ups - MinUPS);
                    }
                    ups = MinUPS;
                    break;
                }
        }
        if (skipTick != 0)
        {
            Log.Debug($"Game Tick desync. skip={skipTick} diff={diff,2}, RTT={rtt}ms, UPS={unitsPerSecond:F2}(avg{averageUPS:F2})");
            GameMain.gameTick += skipTick;
        }
        FPSController.SetFixUPS(ups);
        hasChanged = true;
        // Tick difference in the next second. Expose for other mods
        NotifyTickDifference(diff / 1f + averageUPS - ups);
    }

#pragma warning disable IDE0060
    public static void NotifyTickDifference(float delta)
#pragma warning restore IDE0060
    {
    }

    public static void DoFastReconnect()
    {
        // trigger game exit to main menu
        DuringReconnect = true;
        UIRoot.instance.uiGame.escMenu.OnButton5Click();
    }

    public static void UpdateBufferLength(int length)
    {
        if (length <= 0)
        {
            return;
        }
        bufferLength = length;
        Multiplayer.Session.World.UpdatePingIndicator(LoadingMessage());
    }

    public static string LoadingMessage()
    {
        var progress = bufferLength * 100f / FragmentSize;
        return $"Downloading {FragmentSize / 1000:n0} KB ({progress:F1}%)";
    }

    public void ImportGlobalGameData(GlobalGameDataResponse packet)
    {
        switch (packet.DataType)
        {
            case GlobalGameDataResponse.EDataType.History:
                historyBinaryData = packet.BinaryData;
                Log.Info("Waiting for GalacticTransport data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.GalacticTransport:
                galacticTransportBinaryData = packet.BinaryData;
                Log.Info("Waiting for SpaceSector data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.SpaceSector:
                spaceSectorBinaryData = packet.BinaryData;
                Log.Info("Waiting for MilestoneSystem data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.MilestoneSystem:
                milestoneSystemBinaryData = packet.BinaryData;
                Log.Info("Waiting for TrashSystem data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.TrashSystem:
                trashSystemBinaryData = packet.BinaryData;
                Log.Info("Waiting for GalacticDigital data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.GalacticDigital:
                galacticDigitalBinaryData = packet.BinaryData;
                Log.Info("Waiting for the remaining data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.Ready:
                using (var reader = new BinaryUtils.Reader(packet.BinaryData))
                {
                    var br = reader.BinaryReader;
                    sandboxToolsEnabled = br.ReadBoolean();
                }
                Log.Info("Loading GlobalGameData complete. Initializing...");
                // We are ready to start the game now
                DSPGame.StartGameSkipPrologue(DSPGame.GameDesc);
                break;
        }
    }

    public void OverwriteGlobalGameData(GameData data)
    {
        if (data == null)
        {
            return;
        }

        if (historyBinaryData != null)
        {
            Log.Info("Parsing History data from the server...");
            GameMain.sandboxToolsEnabled = sandboxToolsEnabled;

            if (data.history != null)
            {
                if (data.history.featureKeys != null)
                {
                    foreach (int key in data.history.featureKeys)
                    {
                        PreserveFeatureKey(key);
                    }
                }
                if (data.history.tutorialUnlocked != null)
                {
                    foreach (int tutorialId in data.history.tutorialUnlocked)
                    {
                        PreserveTutorial(tutorialId);
                    }
                }
            }

            if (data.history == null)
            {
                data.history = new GameHistoryData();
            }

            data.history.Init(data);
            using (var reader = new BinaryUtils.Reader(historyBinaryData))
            {
                data.history.Import(reader.BinaryReader);
            }
            historyBinaryData = null;

            data.history.featureKeys ??= new HashSet<int>();
            data.history.tutorialUnlocked ??= new HashSet<int>();

            if (preservedFeatureKeys.Count > 0)
            {
                var keysToRestore = new List<int>(preservedFeatureKeys);
                foreach (int key in keysToRestore)
                {
                    if (!data.history.HasFeatureKey(key))
                    {
                        data.history.RegFeatureKey(key);
                    }
                }
            }
            if (preservedTutorialUnlocked.Count > 0)
            {
                var tutorialsToRestore = new List<int>(preservedTutorialUnlocked);
                foreach (int tutorialId in tutorialsToRestore)
                {
                    if (tutorialId > 0 && !data.history.TutorialUnlocked(tutorialId))
                    {
                        data.history.UnlockTutorial(tutorialId);
                    }
                }
            }

            if (data.history.featureKeys != null)
            {
                foreach (int key in data.history.featureKeys)
                {
                    PreserveFeatureKey(key);
                }
            }
            if (data.history.tutorialUnlocked != null)
            {
                foreach (int tutorialId in data.history.tutorialUnlocked)
                {
                    PreserveTutorial(tutorialId);
                }
            }

            using (Multiplayer.Session.History.IsIncomingRequest.On())
            {
                if (data.history.featureKeys != null)
                {
                    int maxAdvisorUsedKey = FeatureID.ADVISOR_TIP_USED_START + (FeatureID.ADVISOR_TIP_USED_START - FeatureID.ADVISOR_TIP_START);
                    foreach (int key in data.history.featureKeys)
                    {
                        if (key >= FeatureID.ADVISOR_TIP_START && key < FeatureID.ADVISOR_TIP_USED_START)
                        {
                            int tipId = key - FeatureID.ADVISOR_TIP_START;
                            GameMain.gameScenario?.advisorLogic?.SetAdvisorTipFinished(tipId);
                        }
                        else if (key >= FeatureID.ADVISOR_TIP_USED_START && key < maxAdvisorUsedKey)
                        {
                            int tipId = key - FeatureID.ADVISOR_TIP_USED_START;
                            GameMain.gameScenario?.advisorLogic?.SetAdvisorTipUsed(tipId);
                        }
                    }
                }
            }

            var advisorTip = UIRoot.instance?.uiGame?.advisorTip;
            if (advisorTip != null)
            {
                if (advisorTip.playingTip != null &&
                    (data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_START + advisorTip.playingTip.ID) ||
                     data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_USED_START + advisorTip.playingTip.ID)))
                {
                    advisorTip.StopAdvisorTip();
                }
                advisorTip.requests?.RemoveAll(id =>
                    data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_START + id) ||
                    data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_USED_START + id));
                if (advisorTip.nextTip != null &&
                    (data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_START + advisorTip.nextTip.ID) ||
                     data.history.HasFeatureKey(FeatureID.ADVISOR_TIP_USED_START + advisorTip.nextTip.ID)))
                {
                    advisorTip.nextTip = null;
                }
            }

            var tutorialTip = UIRoot.instance?.uiGame?.tutorialTip;
            if (tutorialTip != null && tutorialTip.entryShowed != null)
            {
                for (int i = tutorialTip.entryShowed.Count - 1; i >= 0; i--)
                {
                    var entry = tutorialTip.entryShowed[i];
                    if (entry != null && data.history.TutorialUnlocked(entry.tutorialId))
                    {
                        tutorialTip.CloseTip(entry.tutorialId);
                    }
                }
            }
        }
        if (galacticTransportBinaryData != null)
        {
            Log.Info("Parsing GalacticTransport data from the server...");
            data.galacticTransport.Init(data);
            using (var reader = new BinaryUtils.Reader(galacticTransportBinaryData))
            {
                data.galacticTransport.Import(reader.BinaryReader);
            }
            galacticTransportBinaryData = null;
        }
        if (spaceSectorBinaryData != null)
        {
            Log.Info("Parsing SpaceSector data from the server...");
            using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
            {
                Combat.CombatManager.SerializeOverwrite = true;
                data.spaceSector.isCombatMode = data.gameDesc.isCombatMode;
                using (var reader = new BinaryUtils.Reader(spaceSectorBinaryData))
                {
                    // Re-init will cause some issues, so just overwrite the data with import
                    data.spaceSector.Import(reader.BinaryReader);
                }
                data.mainPlayer.mecha.CheckCombatModuleDataIsValidPatch();
                Combat.CombatManager.SerializeOverwrite = false;
            }
            spaceSectorBinaryData = null;
        }
        if (milestoneSystemBinaryData != null)
        {
            Log.Info("Parsing MilestoneSystem data from the server...");
            data.milestoneSystem.Init(data);
            using (var reader = new BinaryUtils.Reader(milestoneSystemBinaryData))
            {
                data.milestoneSystem.Import(reader.BinaryReader);
            }
            milestoneSystemBinaryData = null;
        }
        if (trashSystemBinaryData != null)
        {
            Log.Info("Parsing TrashSystem data from the server...");
            using (var reader = new BinaryUtils.Reader(trashSystemBinaryData))
            {
                data.trashSystem.Import(reader.BinaryReader);
            }
            // Wait until WarningDataPacket to assign warningId
            var container = data.trashSystem.container;
            for (var i = 0; i < container.trashCursor; i++)
            {
                container.trashDataPool[i].warningId = -1;
            }
            trashSystemBinaryData = null;
        }
        if (galacticDigitalBinaryData != null)
        {
            Log.Info("Parsing GalacticDigital data from the server...");
            using (var reader = new BinaryUtils.Reader(galacticDigitalBinaryData))
            {
                data.galacticDigital.Import(reader.BinaryReader);
            }
            data.galacticDigital.Arragement();
            galacticDigitalBinaryData = null;
        }

        if (PreservedReplicatorMultipliers != null && PreservedReplicatorMultipliers.Length > 0)
        {
            ApplyReplicatorMultipliers(PreservedReplicatorMultipliers);
        }

        if (Planet.PlanetManager.PreservedDashboardData != null && Planet.PlanetManager.PreservedDashboardData.Length > 0 && data.statistics?.charts != null)
        {
            try
            {
                using var reader = new BinaryUtils.Reader(Planet.PlanetManager.PreservedDashboardData);
                data.statistics.charts.Import(reader.BinaryReader);
                var dashboard = UIRoot.instance?.uiGame?.dashboard;
                if (dashboard != null)
                {
                    dashboard.DetermineCharts();
                    dashboard.UpdateCharts();
                }
            }
            catch (Exception e)
            {
                Log.Warn($"Failed to restore dashboard data in OverwriteGlobalGameData: {e}");
            }
        }

        var currentStar = data.localStar ?? data.localPlanet?.star;
        if (currentStar != null)
        {
            PlanetModelingManager.RequestLoadStar(currentStar);
        }
    }

}
