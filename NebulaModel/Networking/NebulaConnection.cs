using LiteNetLib.Utils;
using NebulaModel.Logger;
using WebSocketSharp;

namespace NebulaModel.Networking
{
    public class NebulaConnection
    {
        private readonly WebSocket peerSocket;
        private readonly NetPacketProcessor packetProcessor;

        //public ushort Id => (ushort)peer.Id;
        //public int Ping => peer.Ping;
        //public IPEndPoint Endpoint => peerSocket.UserEndPoint;

        public NebulaConnection(WebSocket peerSocket, NetPacketProcessor packetProcessor)
        {
            this.peerSocket = peerSocket;
            this.packetProcessor = packetProcessor;
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            if(peerSocket.ReadyState == WebSocketState.Open)
            {
                peerSocket.Send(packetProcessor.Write(packet));
            }
            else
            {
                Log.Info($"Cannot send packet {packet?.GetType()} to a closed connection {peerSocket?.Url}");
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
            return Equals((NebulaConnection)obj);
        }

        public override int GetHashCode()
        {
            // TODO: Check if this works
            return peerSocket?.GetHashCode() ?? 0;
        }

        protected bool Equals(NebulaConnection other)
        {
            // TODO: Check if this works
            return peerSocket == other.peerSocket;
        }
    }
}
