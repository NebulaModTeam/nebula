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
        private class Fragment
        {
            public int Remaining;
            public byte[] Data;
        }

        private readonly NetworkingSockets sockets;
        private readonly EndPoint peerEndpoint;
        private readonly uint peerSocket;
        private readonly NetPacketProcessor packetProcessor;
        private readonly Queue<byte[]> sendQueue = new Queue<byte[]>();
        private Dictionary<uint, Fragment> fragments = new Dictionary<uint, Fragment>();
        private uint nextFragmentId = 0;

        private const int KMaxPacketSize = Library.maxMessageSize - 1;
        private const int KMaxFragmentSize = 1 << 16;

        public bool IsAlive
        {
            get
            {
                ConnectionStatus status = new ConnectionStatus();
                sockets.GetQuickConnectionStatus(peerSocket, ref status);

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
            while(sendQueue.Count > 0)
            {
                if (SendImmediateRawPacket(sendQueue.Dequeue()) == false)
                    break;
            }
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if (IsAlive)
            {
                SendRawPacket(packetProcessor.Write(packet));
            }
            else
            {
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peerEndpoint}");
            }
        }

        public bool SendRawPacket(byte[] rawData)
        {
            if (IsAlive)
            {
                if (rawData.Length >= KMaxPacketSize)
                {
                    FragmentPacket(rawData);
                }
                else
                {
                    byte[] data = new byte[rawData.Length + 1];
                    data[0] = 0;
                    Array.Copy(rawData, 0, data, 1, rawData.Length);
                    var result = sockets.SendMessageToConnection(peerSocket, data);
                    if (result == Result.LimitExceeded)
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

        private bool SendImmediateRawPacket(byte[] rawData)
        {
            var result = sockets.SendMessageToConnection(peerSocket, rawData);
            if (result == Result.LimitExceeded)
            {
                sendQueue.Enqueue(rawData);
            }
            else if (result != Result.OK)
            {
                Log.Error($"Cannot send raw data because of error {result}");
            }
            else
                return true;
            return false;
        }

        private void FragmentPacket(byte[] rawData)
        {
            NetDataWriter writer = new NetDataWriter();
            var fragmentId = nextFragmentId++;

            for (var index = 0; index < rawData.Length; index += KMaxFragmentSize)
            {
                writer.Reset();

                writer.Put((byte)1);
                writer.Put(fragmentId);
                writer.Put((int)rawData.Length);
                writer.Put((uint)index);

                var dataToSend = rawData.Length - index;
                var dataChunk = dataToSend > KMaxFragmentSize ? KMaxFragmentSize : dataToSend;

                writer.PutBytesWithLength(rawData, index, dataChunk);

                SendImmediateRawPacket(writer.CopyData());
            }
        }

        public byte[] Receive(byte[] rawData)
        {
            byte[] payload = rawData.Skip(1).ToArray();

            if (rawData[0] == 0)
            {
                return payload;
            }
            else
            {
                var data = ProcessFragment(payload);
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
            var fragId = reader.GetUInt();
            var totalLength = reader.GetInt();
            var offset = reader.GetUInt();
            var data = reader.GetBytesWithLength();

            Fragment frag;
            if(!fragments.TryGetValue(fragId, out frag))
            {
                frag = new Fragment();
                frag.Data = new byte[totalLength];
                frag.Remaining = totalLength;
                fragments.Add(fragId, frag);
            }

            frag.Remaining -= data.Length;
            Array.Copy(data, 0, frag.Data, offset, data.Length);

            if (frag.Remaining > 0)
                return null;

            fragments.Remove(fragId);

            return frag.Data;
        }

        public void Disconnect(DisconnectionReason reason = DisconnectionReason.Normal, string reasonString = null)
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
