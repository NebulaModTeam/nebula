using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;
using System.Net;
using WebSocketSharp;

namespace NebulaModel.Networking
{
    public class NebulaConnection
    {
        private readonly EndPoint peerEndpoint;
        private readonly WebSocket peerSocket;
        private readonly NetPacketProcessor packetProcessor;

        public bool IsAlive => peerSocket?.IsAlive ?? false;

        public NebulaConnection(WebSocket peerSocket, EndPoint peerEndpoint, NetPacketProcessor packetProcessor)
        {
            this.peerEndpoint = peerEndpoint;
            this.peerSocket = peerSocket;
            this.packetProcessor = packetProcessor;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if (peerSocket.ReadyState == WebSocketState.Open)
            {
                peerSocket.Send(packetProcessor.Write(packet));
            }
            else
            {
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peerEndpoint}");
            }
        }

        public void SendRawPacket(byte[] rawData)
        {
            if (peerSocket.ReadyState == WebSocketState.Open)
            {
                peerSocket.Send(rawData);
            }
            else
            {
                Log.Warn($"Cannot send raw packet to a closed connection {peerSocket?.Url}");
            }
        }

        public void Disconnect(DisconnectionReason reason = DisconnectionReason.Normal, string reasonString = null)
        {
            if(string.IsNullOrEmpty(reasonString))
            {
                peerSocket.Close((ushort)reason);
            }
            else
            {
                if (System.Text.Encoding.UTF8.GetBytes(reasonString).Length <= 123)
                {
                    peerSocket.Close((ushort)reason, reasonString);
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
            return (obj as NebulaConnection).peerEndpoint.Equals(this.peerEndpoint);
        }

        public override int GetHashCode()
        {
            return peerEndpoint?.GetHashCode() ?? 0;
        }
    }
}
