using System;
using System.Collections.Generic;
using System.Diagnostics;
using NebulaAPI.Interfaces;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using NebulaModel.Logger;

namespace NebulaModel.Networking.Serialization;

public class NebulaNetPacketProcessor : NetPacketProcessor, INetPacketProcessor
{
    // Packet simulation stuff
    private readonly Dictionary<ulong, Type> _callbacksDebugInfo = [];
    private readonly NetDataWriter writer = new();
    private readonly List<DelayedPacket> delayedPackets = [];
    private readonly Queue<PendingPacket> pendingPackets = new();

#if DEBUG
    private readonly Random simulationRandom = new();
    private readonly int SimulatedMaxLatency = 50;
    private readonly int SimulatedMinLatency = 20;
#endif

    public bool SimulateLatency { get; set; } = false;

    /// <summary>
    /// Whether or not packet processing is enabled
    /// </summary>
    public bool EnablePacketProcessing { get; set; } = true;

    public NebulaNetPacketProcessor()
    {
        _netSerializer = new NebulaNetSerializer();
    }

    /// <summary>
    /// Adds back some functionality that nebula relied on before the update.
    /// This method was removed from LiteNetLib as it was not thread-safe, and is still not thread safe in below implementation.
    /// @TODO: Optimize & move into `NebulaConnection.cs`
    /// </summary>
    public byte[] Write<T>(T packet) where T : class, new()
    {
        writer.Reset();
        Write(writer, packet);

#if DEBUG
        if (!typeof(T).IsDefined(typeof(HidePacketInDebugLogsAttribute), false))
        {
            Log.Debug($"Packet Sent << {packet.GetType().Name}, Size: {writer.Length}");
        }
#endif

        return writer.CopyData();
    }

    #region DEBUG_PACKET_DELAY

    public void ProcessPacketQueue()
    {
        lock (pendingPackets)
        {
            ProcessDelayedPackets();

            while (pendingPackets.Count > 0 && EnablePacketProcessing)
            {
                var packet = pendingPackets.Dequeue();
                try
                {
                    ReadPacket(new NetDataReader(packet.Data), packet.UserData);
                }
                catch (Exception ex)
                {
                    // We can't do anything to ParseException, so just leave a warning in the log file
                    // For other exception, display a red error pop-up window
                    if (ex is not ParseException)
                    {
                        Log.Error(ex);
                    }
                }
            }
        }
    }

    [Conditional("DEBUG")]
    private void ProcessDelayedPackets()
    {
        lock (delayedPackets)
        {
            var now = DateTime.UtcNow;
            var deleteCount = 0;

            for (var i = 0; i < delayedPackets.Count; ++i)
            {
                if (now >= delayedPackets[i].DueTime)
                {
                    pendingPackets.Enqueue(delayedPackets[i].Packet);
                    deleteCount = i + 1;
                }
                else
                {
                    // We need to break to avoid messing up the order of the packets.
                    break;
                }
            }

            if (deleteCount > 0)
            {
                delayedPackets.RemoveRange(0, deleteCount);
            }
        }
    }

    public void EnqueuePacketForProcessing<T>(T packet, object userData) where T : class, new()
    {
        EnqueuePacketForProcessing(Write(packet), userData);
    }

    public void EnqueuePacketForProcessing(byte[] rawData, object userData)
    {
#if DEBUG
        if (SimulateLatency)
        {
            lock (delayedPackets)
            {
                var packet = new PendingPacket(rawData, userData);
                var dueTime = DateTime.UtcNow.AddMilliseconds(simulationRandom.Next(SimulatedMinLatency, SimulatedMaxLatency));
                delayedPackets.Add(new DelayedPacket(packet, dueTime));
                return;
            }
        }
#endif
        lock (pendingPackets)
        {
            pendingPackets.Enqueue(new PendingPacket(rawData, userData));
        }
    }

    #endregion
}
