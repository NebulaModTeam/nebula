﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using NebulaAPI.Interfaces;

namespace NebulaModel.Networking.Serialization;

public partial class NetPacketProcessor
{
    // Packet simulation stuff
    private readonly Dictionary<ulong, Type> _callbacksDebugInfo = [];
    private readonly NetDataWriter _netDataWriter = new();
    private readonly List<DelayedPacket> delayedPackets = [];
    private readonly Queue<PendingPacket> pendingPackets = new();

    private readonly Random simulationRandom = new();
    private readonly int SimulatedMaxLatency = 50;
    private readonly int SimulatedMinLatency = 20;

    public bool SimulateLatency = false;

    /// <summary>
    /// Whether or not packet processing is enabled
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    ///  
    /// </summary>
    private static readonly NetDataWriter writer = new();

    /// <summary>
    /// Adds back some functionality that nebula relied on before the update.
    /// This method was removed from LiteNetLib as it was not thread-safe, and is still not thread safe in below implementation.
    /// @TODO: Optimize & move into `NebulaConnection.cs`
    /// </summary>
    public byte[] Write<T>(T packet) where T : class, new()
    {
        writer.Reset();
        Write(writer, packet);
        return writer.CopyData();
    }

    #region DEBUG_PACKET_DELAY

    public void ProcessPacketQueue()
    {
        lock (pendingPackets)
        {
            ProcessDelayedPackets();

            while (pendingPackets.Count > 0 && Enable)
            {
                var packet = pendingPackets.Dequeue();
                ReadPacket(new NetDataReader(packet.Data), packet.UserData);
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
            }
        }
        else
        {
            lock (pendingPackets)
            {
                pendingPackets.Enqueue(new PendingPacket(rawData, userData));
            }
        }
#else
        lock (pendingPackets)
        {
            pendingPackets.Enqueue(new PendingPacket(rawData, userData));
        }
#endif
    }

    #endregion
}

public static class NetPacketProcessorExtension
{
}