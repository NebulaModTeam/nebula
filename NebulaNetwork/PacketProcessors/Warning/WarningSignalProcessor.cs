#region

using System;
using NebulaAPI.Packets;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Warning;
using NebulaWorld;

#endregion

namespace NebulaNetwork.PacketProcessors.Warning;

[RegisterPacketProcessor]
internal class WarningSignalProcessor : PacketProcessor<WarningSignalPacket>
{
    protected override void ProcessPacket(WarningSignalPacket packet, NebulaConnection conn)
    {
        var ws = GameMain.data.warningSystem;
        Array.Clear(ws.warningCounts, 0, ws.warningCounts.Length);
        Array.Clear(ws.warningSignals, 0, ws.warningSignalCount);

        ws.warningSignalCount = packet.SignalCount;
        for (var i = 0; i < packet.SignalCount; i++)
        {
            var signalId = packet.Signals[i];
            ws.warningSignals[i] = signalId;
            ws.warningCounts[signalId] = packet.Counts[i];
        }

        Multiplayer.Session.Warning.TickSignal = packet.Tick;
    }
}
