using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Valve.Sockets;

namespace NebulaModel.Networking
{
    public class NebulaConnection : INebulaConnection
    {
        private class FragmentedPayload
        {
            public int Remaining;
            public byte[] Data;
        }

        private readonly NetworkingSockets sockets;
        private readonly EndPoint peerEndpoint;
        private readonly uint peerSocket;
        private readonly NetPacketProcessor packetProcessor;
        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private Dictionary<uint, FragmentedPayload> fragmentedPayloads = new Dictionary<uint, FragmentedPayload>();
        private uint nextFragmentId = 0;

        private const int KMaxPacketSize = Library.maxMessageSize - 1;
        private const int KMaxFragmentSize = 1 << 16;

        public bool IsAlive
        {
            get
            {
                ConnectionStatus status = new ConnectionStatus();
                lock (sockets)
                {
                    sockets.GetQuickConnectionStatus(peerSocket, ref status);
                }

                return status.state == ConnectionState.Connected;
            }
        }

        public NebulaConnection(NetworkingSockets sockets, uint peerSocket, EndPoint peerEndpoint, NetPacketProcessor packetProcessor)
        {
            this.sockets = sockets;
            this.peerEndpoint = peerEndpoint;
            this.peerSocket = peerSocket;
            this.packetProcessor = packetProcessor;
        }

        public void Update()
        {
            // Try to send the first packet, if we did, remove it
            while(sendQueue.Count > 0 && SendImmediateRawPacket(sendQueue.Peek()) == Result.OK)
            {
                sendQueue.Dequeue();
            }
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if (IsAlive)
            {
                var netAttribute = typeof(T).GetCustomAttributes(typeof(NetworkOptionsAttribute), false).FirstOrDefault() as NetworkOptionsAttribute;

                // By default we send everything as reliable data
                SendFlags sendFlags = SendFlags.Reliable;

                // Packets can also specify explicitly if they are reliable or not
                if (netAttribute != null && netAttribute.Reliable == false)
                    sendFlags = SendFlags.Unreliable;

                SendRawPacket(packetProcessor.Write(packet), sendFlags);
            }
            else
            {
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peerEndpoint}");
            }
        }

        public bool SendRawPacket(byte[] rawData, SendFlags sendFlags = SendFlags.Reliable)
        {
            if (IsAlive)
            {
                // Valve's net lib has restrictions on packet size much larger than this (512KB) 
                // but we fragment into smaller pieces to prevent blocking other packets from going
                // through
                if (rawData.Length >= KMaxPacketSize)
                {
                    FragmentPacket(rawData);
                }
                else
                {
                    // We prefix the data with a 0 byte as this is not a fragment
                    byte[] data = new byte[rawData.Length + 1];
                    data[0] = 0;
                    Array.Copy(rawData, 0, data, 1, rawData.Length);

                    Result result = Result.LimitExceeded;

                    // If we are trying to send a reliable packet and we have packets queued, queue the packet to preserve send order
                    if (sendQueue.Count == 0 || sendFlags != SendFlags.Reliable)
                    {
                        lock (sockets)
                        {
                            result = sockets.SendMessageToConnection(peerSocket, data, sendFlags);
                        }
                    }

                    // If the underlying send queue is full and we are not dealing with an unreliable packet, queue it for resend
                    if (result == Result.LimitExceeded && sendFlags != SendFlags.Unreliable)
                    {
                        sendQueue.Enqueue(data);
                    }
                    else if (result != Result.OK)
                    {
                        Log.Error($"Cannot send raw data because of error {result}");
                    }
                    else
                        return true;
                }
            }
            else
            {
                Log.Warn($"Cannot send raw packet to a closed connection {peerEndpoint}");
            }

            return false;
        }

        private Result SendImmediateRawPacket(byte[] rawData)
        {
            Result result = Result.Fail;
            lock (sockets)
            {
                result = sockets.SendMessageToConnection(peerSocket, rawData, SendFlags.Reliable);
            }

            // All immediate sends are reliable so queue them if we couldn't send them right now
            if (result == Result.LimitExceeded)
            {
                return Result.LimitExceeded;
            }
            else if (result != Result.OK)
            {
                Log.Error($"Cannot send raw data because of error {result}");
            }
            else
                return Result.OK;

            return Result.Fail;
        }

        private void FragmentPacket(byte[] rawData)
        {
            NetDataWriter writer = new NetDataWriter();
            var fragmentId = nextFragmentId++;

            for (var index = 0; index < rawData.Length; index += KMaxFragmentSize)
            {
                writer.Reset();

                // We prefix the data with a 1 byte as this is a fragment
                writer.Put((byte)1);
                writer.Put(fragmentId);
                writer.Put((int)rawData.Length);
                writer.Put((uint)index);

                var dataToSend = rawData.Length - index;
                var dataChunk = dataToSend > KMaxFragmentSize ? KMaxFragmentSize : dataToSend;

                writer.PutBytesWithLength(rawData, index, dataChunk);

                // Try to send fragments as they are processed, if we fail to send it will be queued for later send
                var data = writer.CopyData();
                if (SendImmediateRawPacket(data) == Result.LimitExceeded)
                    sendQueue.Enqueue(data);
            }
        }

        public byte[] Receive(byte[] rawData)
        {
            byte[] payload = rawData.Skip(1).ToArray();

            // Not a fragment, return the data for processing
            if (rawData[0] == 0)
            {
                return payload;
            }
            // Fragment, use it to reconstruct the packet
            else
            {
                var data = ProcessFragment(payload);
                // If the processed fragment was the last missing piece, we get the full packet, return it for processing
                if (data != null)
                {
                    return data;
                }
            }

            return null;
        }

        private byte[] ProcessFragment(byte[] payload)
        {
            NetDataReader reader = new NetDataReader(payload);
            var fragmentId = reader.GetUInt();
            var totalLength = reader.GetInt();
            var offset = reader.GetUInt();
            var data = reader.GetBytesWithLength();

            FragmentedPayload fragmentedPayload;
            if(!fragmentedPayloads.TryGetValue(fragmentId, out fragmentedPayload))
            {
                // This fragment is for a packet we do not know yet, create it
                fragmentedPayload = new FragmentedPayload();
                fragmentedPayload.Data = new byte[totalLength];
                fragmentedPayload.Remaining = totalLength;
                fragmentedPayloads.Add(fragmentId, fragmentedPayload);
            }

            fragmentedPayload.Remaining -= data.Length;
            Array.Copy(data, 0, fragmentedPayload.Data, offset, data.Length);

            // We have filled all the gaps, return the data
            if (fragmentedPayload.Remaining > 0)
                return null;

            fragmentedPayloads.Remove(fragmentId);

            return fragmentedPayload.Data;
        }

        public void Disconnect(DisconnectionReason reason = DisconnectionReason.Normal, string reasonString = null)
        {
            lock (sockets)
            {
                if (string.IsNullOrEmpty(reasonString))
                {
                    sockets.CloseConnection(peerSocket, (int)reason, "", true);
                }
                else
                {
                    if (System.Text.Encoding.UTF8.GetBytes(reasonString).Length <= Library.maxCloseMessageLength)
                    {
                        sockets.CloseConnection(peerSocket, (int)reason, reasonString, true);
                    }
                    else
                    {
                        throw new ArgumentException("Reason string cannot take up more than 123 bytes");
                    }
                }
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
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return (obj as NebulaConnection).peerEndpoint.Equals(peerEndpoint);
        }

        public override int GetHashCode()
        {
            return peerEndpoint?.GetHashCode() ?? 0;
        }

        
    }
}
