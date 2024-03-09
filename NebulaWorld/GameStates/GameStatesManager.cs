#region

using System;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;

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
    public static GameDesc NewGameDesc { get; set; }
    public static int FragmentSize { get; set; }

    // Store data get from GlobalGameDataResponse
    static bool SandboxToolsEnabled { get; set; }
    static byte[] HistoryBinaryData { get; set; }
    static byte[] SpaceSectorBinaryData { get; set; }
    static byte[] MilestoneSystemBinaryData { get; set; }
    static byte[] TrashSystemBinaryData { get; set; }


    public void Dispose()
    {
        FragmentSize = 0;
        SandboxToolsEnabled = false;
        HistoryBinaryData = null;
        SpaceSectorBinaryData = null;
        MilestoneSystemBinaryData = null;
        TrashSystemBinaryData = null;
        GC.SuppressFinalize(this);
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

    public static void ImportGlobalGameData(GlobalGameDataResponse packet)
    {
        switch (packet.DataType)
        {
            case GlobalGameDataResponse.EDataType.History:
                HistoryBinaryData = packet.BinaryData;
                Log.Info("Waiting for SpaceSector data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.SpaceSector:
                SpaceSectorBinaryData = packet.BinaryData;
                Log.Info("Waiting for MilestoneSystem data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.MilestoneSystem:
                MilestoneSystemBinaryData = packet.BinaryData;
                Log.Info("Waiting for TrashSystem data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.TrashSystem:
                TrashSystemBinaryData = packet.BinaryData;
                Log.Info("Waiting for the remaining data from the server...");
                break;

            case GlobalGameDataResponse.EDataType.Ready:
                using (var reader = new BinaryUtils.Reader(packet.BinaryData))
                {
                    var br = reader.BinaryReader;
                    SandboxToolsEnabled = br.ReadBoolean();
                }
                Log.Info("Loading GlobalGameData complete. Initializing...");
                // We are ready to start the game now
                DSPGame.StartGameSkipPrologue(DSPGame.GameDesc);
                break;
        }
    }

    public static void OverwriteGlobalGameData(GameData data)
    {
        if (HistoryBinaryData != null)
        {
            Log.Info("Parsing History data from the server...");
            GameMain.sandboxToolsEnabled = SandboxToolsEnabled;
            data.history.Init(data);
            using (var reader = new BinaryUtils.Reader(HistoryBinaryData))
            {
                data.history.Import(reader.BinaryReader);
            }
            HistoryBinaryData = null;
        }
        if (SpaceSectorBinaryData != null)
        {
            Log.Info("Parsing SpaceSector data from the server...");
            using (Multiplayer.Session.Enemies.IsIncomingRequest.On())
            {
                Combat.CombatManager.SerializeOverwrite = true;
                data.spaceSector.isCombatMode = data.gameDesc.isCombatMode;
                using (var reader = new BinaryUtils.Reader(SpaceSectorBinaryData))
                {
                    // Re-init will cause some issues, so just overwrite the data with import
                    data.spaceSector.Import(reader.BinaryReader);
                }
                data.mainPlayer.mecha.CheckCombatModuleDataIsValidPatch();
                Combat.CombatManager.SerializeOverwrite = false;
            }
            SpaceSectorBinaryData = null;
        }
        if (MilestoneSystemBinaryData != null)
        {
            Log.Info("Parsing MilestoneSystem data from the server...");
            data.milestoneSystem.Init(data);
            using (var reader = new BinaryUtils.Reader(MilestoneSystemBinaryData))
            {
                data.milestoneSystem.Import(reader.BinaryReader);
            }
            MilestoneSystemBinaryData = null;
        }
        if (TrashSystemBinaryData != null)
        {
            Log.Info("Parsing TrashSystem data from the server...");
            using (var reader = new BinaryUtils.Reader(TrashSystemBinaryData))
            {
                data.trashSystem.Import(reader.BinaryReader);
            }
            // Wait until WarningDataPacket to assign warningId
            var container = data.trashSystem.container;
            for (var i = 0; i < container.trashCursor; i++)
            {
                container.trashDataPool[i].warningId = -1;
            }
            TrashSystemBinaryData = null;
        }
    }
}
