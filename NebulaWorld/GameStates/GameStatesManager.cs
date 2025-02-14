#region

using System;
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
    private static int bufferLength;

    public static long RealGameTick => GameMain.gameTick;
    public static float RealUPS => (float)FPSController.currentUPS;
    public static string ImportedSaveName { get; set; }
    public static long LastSaveTime { get; set; } // UnixTimeSeconds
    public static GameDesc NewGameDesc { get; set; }
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

    public GameStatesManager()
    {
        LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void Dispose()
    {
        LastSaveTime = FragmentSize = 0;
        sandboxToolsEnabled = false;
        historyBinaryData = null;
        galacticTransportBinaryData = null;
        spaceSectorBinaryData = null;
        milestoneSystemBinaryData = null;
        trashSystemBinaryData = null;
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
        if (historyBinaryData != null)
        {
            Log.Info("Parsing History data from the server...");
            GameMain.sandboxToolsEnabled = sandboxToolsEnabled;
            data.history.Init(data);
            using (var reader = new BinaryUtils.Reader(historyBinaryData))
            {
                data.history.Import(reader.BinaryReader);
            }
            historyBinaryData = null;
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
    }
}
