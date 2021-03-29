using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System.Net;
using WebSocketSharp;

namespace NebulaModel.Networking
{
    public class NebulaConnection
    {
        private readonly IPEndPoint peerEndpoint;
        private readonly WebSocket peerSocket;
        private readonly NetPacketProcessor packetProcessor;

        public bool IsAlive => peerSocket?.IsAlive ?? false;

        public NebulaConnection(WebSocket peerSocket, IPEndPoint peerEndpoint, NetPacketProcessor packetProcessor)
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
                Log.Warn($"Cannot send packet {packet?.GetType()} to a closed connection {peerSocket?.Url}");
            }
        }

        public void Disconnect()
        {
            //TODO: Create a disconnectReason packet type so the users know why the server disconnected them
            peerSocket.Close();
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
