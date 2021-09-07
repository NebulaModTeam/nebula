using NebulaAPI;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System;

namespace NebulaModel.Networking
{
    public class NebulaConnection : INebulaConnection
    {
        private readonly Telepathy.Client client;
        private readonly Telepathy.Server server;
        private readonly NetPacketProcessor packetProcessor;
        private readonly int connectionId;
        private readonly string peerAddress;

        public NebulaConnection(Telepathy.Client client, Telepathy.Server server, NetPacketProcessor packetProcessor, int connectionId = -1)
        {
            this.client = client;
            this.server = server;
            this.packetProcessor = packetProcessor;
            this.connectionId = connectionId;
            peerAddress = server?.GetClientAddress(connectionId) ?? "localhost";
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if (client?.Connected ?? false)
            {
                client.Send(new ArraySegment<byte>(packetProcessor.Write(packet)));
            }
            else if (server?.Active ?? false)
            {
                server.Send(connectionId, new ArraySegment<byte>(packetProcessor.Write(packet)));
            }
            else
            {
                Log.Warn($"Cannot send packet {packet?.GetType()} to closed connection {(connectionId != -1 ? connectionId.ToString() : string.Empty)}");
            }
        }

        public void SendRawPacket(byte[] rawData)
        {
            if (client?.Connected ?? false)
            {
                client.Send(new ArraySegment<byte>(rawData));
            }
            else if (server?.Active ?? false)
            {
                server.Send(connectionId, new ArraySegment<byte>(rawData));
            }
            else
            {
                Log.Warn($"Cannot send raw packet to closed connection {(connectionId != -1 ? connectionId.ToString() : string.Empty)}");
            }
        }

        public void Disconnect(DisconnectionReason reason = DisconnectionReason.Normal, string reasonString = null)
        {
            //if (string.IsNullOrEmpty(reasonString))
            //{
            //    peerSocket.Close((ushort)reason);
            //}
            //else
            //{
            //    if (System.Text.Encoding.UTF8.GetBytes(reasonString).Length <= 123)
            //    {
            //        peerSocket.Close((ushort)reason, reasonString);
            //    }
            //    else
            //    {
            //        throw new ArgumentException("Reason string cannot take up more than 123 bytes");
            //    }
            //}

            server.Disconnect(connectionId);
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
            return (obj as NebulaConnection).peerAddress.Equals(peerAddress);
        }

        public override int GetHashCode()
        {
            return peerAddress?.GetHashCode() ?? 0;
        }
    }
}
