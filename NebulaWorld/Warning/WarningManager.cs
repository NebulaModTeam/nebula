#region

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using BepInEx;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.Warning;

#endregion

namespace NebulaWorld.Warning;

public class WarningManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingMonitorPacket = new();
    private int idleCycle;

    private ConcurrentBag<NebulaConnection> requesters;
    private WarningDataPacket warningDataPacket;
    private WarningSignalPacket warningSignalPacket;
    private WarningSystem ws;

    public WarningManager()
    {
        requesters = new ConcurrentBag<NebulaConnection>();
        warningSignalPacket = new WarningSignalPacket();
        warningSignalPacket.Signals = new int[8];
        warningSignalPacket.Counts = new int[8];
        warningDataPacket = new WarningDataPacket();
    }

    public int TickSignal { get; set; }
    public int TickData { get; set; }
    public long LastRequestTime { get; set; }

    public void Dispose()
    {
        requesters = null;
        warningSignalPacket = null;
        warningDataPacket = null;
    }

    public void HandleRequest(WarningDataRequest packet, NebulaConnection conn)
    {
        if (packet.Event == WarningRequestEvent.Signal)
        {
            conn.SendPacket(warningSignalPacket);
        }
        else
        {
            if (warningSignalPacket.Tick == warningDataPacket.Tick)
            {
                // warningDataPacket is latest, no need to update
                conn.SendPacket(warningDataPacket);
            }
            else
            {
                // Send warningDataPacket after updating is done
                requesters.Add(conn);
            }
        }
    }

    public void SendBroadcastIfNeeded()
    {
        ws = GameMain.data.warningSystem;
        var hasChanged = CheckAndUpdateSignal();
        if (hasChanged)
        {
            warningSignalPacket.Tick = unchecked((int)GameMain.gameTick);
            Multiplayer.Session.Network.SendPacket(warningSignalPacket);
            idleCycle = 0;
        }
        else
        {
            if (++idleCycle > 60)
            {
                // In case of warning content is changed but signal stays the same, force update
                warningSignalPacket.Tick = unchecked((int)GameMain.gameTick);
                Multiplayer.Session.Network.SendPacket(warningSignalPacket);
                idleCycle = 0;
            }
        }
        if (!requesters.IsEmpty)
        {
            using (var writer = new BinaryUtils.Writer())
            {
                warningDataPacket.ActiveWarningCount = ExportBinaryData(writer.BinaryWriter);
                warningDataPacket.BinaryData = writer.CloseAndGetBytes();
                warningDataPacket.Tick = warningSignalPacket.Tick;
            }
            while (requesters.TryTake(out var conn))
            {
                conn.SendPacket(warningDataPacket);
            }
        }
    }

    public bool CheckAndUpdateSignal()
    {
        var hasChanged = warningSignalPacket.SignalCount != ws.warningSignalCount;
        warningSignalPacket.SignalCount = ws.warningSignalCount;
        if (hasChanged)
        {
            warningSignalPacket.SignalCount = ws.warningSignalCount;
            warningSignalPacket.Signals = new int[warningSignalPacket.SignalCount];
            warningSignalPacket.Counts = new int[warningSignalPacket.SignalCount];
        }
        for (var i = 0; i < ws.warningSignalCount; i++)
        {
            var signalId = ws.warningSignals[i];
            if (warningSignalPacket.Signals[i] != signalId || warningSignalPacket.Counts[i] != ws.warningCounts[signalId])
            {
                warningSignalPacket.Signals[i] = signalId;
                warningSignalPacket.Counts[i] = ws.warningCounts[signalId];
                hasChanged = true;
            }
        }

        return hasChanged;
    }

    public int ExportBinaryData(BinaryWriter bw)
    {
        var activeWarningCount = 0;
        var warningPool = ws.warningPool;
        //index start from 1 in warningPool
        for (var i = 1; i < ws.warningCursor; i++)
        {
            var data = warningPool[i];
            if (data.id == i && data.state > 0)
            {
                bw.Write(data.signalId);
                bw.Write(data.detailId);
                bw.Write(data.astroId);
                bw.Write(data.localPos.x);
                bw.Write(data.localPos.y);
                bw.Write(data.localPos.z);

                var trashId = data.factoryId == WarningData.TRASH_SYSTEM ? data.objectId : -1;
                bw.Write(trashId);

                activeWarningCount++;
            }
        }
        return activeWarningCount;
    }

    public void ImportBinaryData(BinaryReader br, int activeWarningCount)
    {
        ws = GameMain.data.warningSystem;
        var newCapacity = ws.warningCapacity;
        while (activeWarningCount + 1 > newCapacity)
        {
            newCapacity *= 2;
        }
        if (newCapacity > ws.warningCapacity)
        {
            ws.SetWarningCapacity(newCapacity);
        }
        ws.warningCursor = activeWarningCount + 1;

        var warningPool = GameMain.data.warningSystem.warningPool;
        //index start from 1 in warningPool
        for (var i = 1; i <= activeWarningCount; i++)
        {
            // factoryId is not synced to skip WarningLogic update in client
            warningPool[i].id = i;
            warningPool[i].state = 1;
            warningPool[i].signalId = br.ReadInt32();
            warningPool[i].detailId = br.ReadInt32();
            // localPos is base on astroId
            warningPool[i].astroId = br.ReadInt32();
            warningPool[i].localPos.x = br.ReadSingle();
            warningPool[i].localPos.y = br.ReadSingle();
            warningPool[i].localPos.z = br.ReadSingle();

            // reassign warningId for trash
            var trashId = br.ReadInt32();
            if (trashId >= 0 && trashId < GameMain.data.trashSystem.container.trashCursor)
            {
                GameMain.data.trashSystem.container.trashDataPool[trashId].warningId = i;
            }
        }
    }

    public static void DisplayTemporaryWarning(string warningText, int millisecond)
    {
        DisplayCriticalWarning(warningText);
        ThreadingHelper.Instance.StartAsyncInvoke(() =>
        {
            Thread.Sleep(millisecond);
            return () =>
            {
                RemoveCriticalWarning();
            };
        });
    }

    public static void DisplayCriticalWarning(string warningText)
    {
        var warningSystem = GameMain.data.warningSystem;
        var id = ECriticalWarning.Null;

        if (warningSystem.criticalWarnings.ContainsKey(id))
        {
            if (warningSystem.criticalWarnings[id].warningParam != 0)
            {
                warningSystem.criticalWarnings[id].warningParam = 0;
                warningSystem.criticalWarnings[id].Update();
                warningSystem.UpdateCriticalWarningText();
            }
        }
        else
        {
            var data = new CriticalWarningData(id, 0);
            data.warningText = warningText;
            warningSystem.criticalWarnings.Add(id, data);
            warningSystem.UpdateCriticalWarningText();
        }
    }

    public static void RemoveCriticalWarning()
    {
        GameMain.data.warningSystem.UnsetCriticalWarning(ECriticalWarning.Null);
    }
}
