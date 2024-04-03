#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using NebulaAPI.Networking;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using WebSocketSharp;

#endregion

namespace NebulaModel.Networking;

public class NebulaConnection : INebulaConnection
{
    private readonly NebulaNetPacketProcessor packetProcessor;

    private readonly EndPoint peerEndpoint;

    // Temporarily public until refactor finished
    public WebSocket peerSocket { get; private set; }

    private readonly Queue<byte[]> pendingPackets = new();
    private bool enable = true;
    private EConnectionStatus connectionStatus = EConnectionStatus.Undefined;

    public bool IsAlive => peerSocket?.IsAlive ?? false;

    public int Id { get; }

    public EConnectionStatus ConnectionStatus
    {
        get => connectionStatus;
        set
        {
            if (value < connectionStatus)
                throw new InvalidOperationException("Connection Status cannot be rolled back to a lower state.");

            connectionStatus = value;
        }
    }

    public NebulaConnection(WebSocket peerSocket, EndPoint peerEndpoint, NebulaNetPacketProcessor packetProcessor)
    {
        this.peerEndpoint = peerEndpoint;
        this.peerSocket = peerSocket;
        this.packetProcessor = packetProcessor;
        this.Id = peerEndpoint.GetHashCode();
    }


    public void SendPacket<T>(T packet) where T : class, new()
    {
        lock (pendingPackets)
        {
            lock (packetProcessor)
            {
                var rawData = packetProcessor.Write(packet); //not-threadsafe
                pendingPackets.Enqueue(rawData);
            }
            ProcessPacketQueue();
        }
    }

    public void SendRawPacket(byte[] rawData)
    {
        lock (pendingPackets)
        {
            pendingPackets.Enqueue(rawData);
            ProcessPacketQueue();
        }
    }

    public bool Equals(INebulaConnection connection)
    {
        return connection != null && ((NebulaConnection)connection).peerEndpoint.Equals(peerEndpoint);
    }

    private void ProcessPacketQueue()
    {
        if (!enable || pendingPackets.Count <= 0)
        {
            return;
        }

        var packet = pendingPackets.Dequeue();
        if (peerSocket.ReadyState == WebSocketState.Open)
        {
            peerSocket.SendAsync(packet, OnSendCompleted);
            enable = false;
        }
        else
        {
            Log.Warn($"Cannot send packet to a {peerSocket.ReadyState} connection {peerEndpoint.GetHashCode()}");
        }
    }

    private void OnSendCompleted(bool result)
    {
        lock (pendingPackets)
        {
            enable = true;
            ProcessPacketQueue();
        }
    }

    public static bool operator ==(NebulaConnection left, NebulaConnection right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(NebulaConnection left, NebulaConnection right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && ((NebulaConnection)obj).peerEndpoint.Equals(peerEndpoint);
    }

    public override int GetHashCode()
    {
        return peerEndpoint?.GetHashCode() ?? 0;
    }
}
