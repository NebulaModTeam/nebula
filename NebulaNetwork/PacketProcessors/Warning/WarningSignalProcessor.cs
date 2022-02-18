﻿using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;
using System;

namespace NebulaNetwork.PacketProcessors.Warning
{
    [RegisterPacketProcessor]
    internal class WarningSignalProcessor : PacketProcessor<WarningSignalPacket>
    {
        public override void ProcessPacket(WarningSignalPacket packet, NebulaConnection conn)
        {
            WarningSystem ws = GameMain.data.warningSystem;
            Array.Clear(ws.warningCounts, 0, ws.warningCounts.Length);
            Array.Clear(ws.warningSignals, 0, ws.warningSignalCount);

            ws.warningSignalCount = packet.SignalCount;
            for (int i = 0; i < packet.SignalCount; i++)
            {
                int signalId = packet.Signals[i];
                ws.warningSignals[i] = signalId;
                ws.warningCounts[signalId] = packet.Counts[i];
            }

            Multiplayer.Session.Warning.TickSignal = packet.Tick;
        }
    }
}
