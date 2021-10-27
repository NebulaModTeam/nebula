using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;
using System.Net;
using Valve.Sockets;

namespace NebulaModel.Networking
{
    public class NebulaConnection : INebulaConnection
    {
        private readonly NetworkingSockets sockets;
        private readonly EndPoint peerEndpoint;
        private readonly uint peerSocket;
        private readonly NetPacketProcessor packetProcessor;

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

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if (IsAlive)
            {
                sockets.SendMessageToConnection(peerSocket, packetProcessor.Write(packet));
            }
            else
            {
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peerEndpoint}");
            }
        }

        public void SendRawPacket(byte[] rawData)
        {
            if (IsAlive)
            {
                sockets.SendMessageToConnection(peerSocket, rawData);
            }
            else
            {
                Log.Warn($"Cannot send raw packet to a closed connection {peerEndpoint}");
            }
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
