using NebulaModel.Networking;
using NebulaModel.Packets.Warning;
using System;
using System.IO;
using System.Collections.Concurrent;
using NebulaModel.DataStructures;

namespace NebulaWorld.Warning
{
    public class WarningManager : IDisposable
    {
        public readonly ToggleSwitch IsIncomingMonitorPacket = new ToggleSwitch();
        public int TickSignal { get; set; }
        public int TickData { get; set; }
        public long LastRequestTime { get; set; }

        private ConcurrentBag<NebulaConnection> requesters;
        private WarningSystem ws;
        private WarningSignalPacket warningSignalPacket;
        private WarningDataPacket warningDataPacket;
        private int idleCycle;

        public WarningManager()
        {            
            requesters = new ConcurrentBag<NebulaConnection>();
            warningSignalPacket = new WarningSignalPacket();
            warningSignalPacket.Signals = new int[8];
            warningSignalPacket.Counts = new int[8];
            warningDataPacket = new WarningDataPacket();
        }

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
            bool hasChanged = CheckAndUpdateSignal();
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
                using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                {
                    warningDataPacket.ActiveWarningCount = ExportBinaryData(writer.BinaryWriter);
                    warningDataPacket.BinaryData = writer.CloseAndGetBytes();
                    warningDataPacket.Tick = warningSignalPacket.Tick;
                }
                while (requesters.TryTake(out NebulaConnection conn))
                {
                    conn.SendPacket(warningDataPacket);
                }
            }
        }

        public bool CheckAndUpdateSignal()
        {
            bool hasChanged = warningSignalPacket.SignalCount != ws.warningSignalCount;
            warningSignalPacket.SignalCount = ws.warningSignalCount;
            if (hasChanged)
            {
                warningSignalPacket.SignalCount = ws.warningSignalCount;
                warningSignalPacket.Signals = new int[warningSignalPacket.SignalCount];
                warningSignalPacket.Counts = new int[warningSignalPacket.SignalCount];
            }
            for (int i = 0; i < ws.warningSignalCount; i++)
            {
                int signalId = ws.warningSignals[i];
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
            int activeWarningCount = 0;   
            WarningData[] warningPool = ws.warningPool;
            for (int i = 0; i < ws.warningCursor; i++)
            {
                WarningData data = warningPool[i];
                if (data.id == i && data.state > 0)
                {
                    bw.Write(data.signalId);
                    bw.Write(data.detailId);
                    bw.Write(data.astroId);
                    bw.Write(data.localPos.x);
                    bw.Write(data.localPos.y);
                    bw.Write(data.localPos.z);
                    activeWarningCount++;
                }                
            }
            return activeWarningCount;
        }

        public void ImporBinaryData(BinaryReader br, int activeWarningCount)
        {
            ws = GameMain.data.warningSystem;
            while (activeWarningCount > ws.warningCapacity)
			{
                ws.warningCapacity *= 2;
			}
            ws.SetWarningCapacity(ws.warningCapacity);
            ws.warningCursor = activeWarningCount;

            //Import warning pool
            WarningData[] warningPool = GameMain.data.warningSystem.warningPool;
            for (int i = 0; i < activeWarningCount; i++)
            {
                warningPool[i].id = i;
                warningPool[i].state = 1;
                warningPool[i].signalId = br.ReadInt32();
                warningPool[i].detailId = br.ReadInt32();
                warningPool[i].astroId = br.ReadInt32();
                warningPool[i].localPos.x = br.ReadSingle();
                warningPool[i].localPos.y = br.ReadSingle();
                warningPool[i].localPos.z = br.ReadSingle();
            }
        }
    }
}
