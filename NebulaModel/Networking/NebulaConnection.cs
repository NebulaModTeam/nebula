using HarmonyLib;
using NebulaModel.Logger;
using NebulaModel.Networking.Serialization;
using System.Reflection;
using WebSocketSharp;

namespace NebulaModel.Networking
{
    public class NebulaConnection
    {
        private readonly WebSocket peerSocket;
        private readonly NetPacketProcessor packetProcessor;

        private readonly FieldInfo webSocket_base64Key = AccessTools.Field(typeof(WebSocket), "_base64Key");

        public bool IsAlive => peerSocket?.IsAlive ?? false;

        public NebulaConnection(WebSocket peerSocket, NetPacketProcessor packetProcessor)
        {
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
            return peerSocket == null ? 0 : ((string)webSocket_base64Key.GetValue(peerSocket)).GetHashCode();
        }

        protected bool Equals(NebulaConnection other)
        {
            return string.Equals((string)webSocket_base64Key.GetValue(peerSocket), (string)webSocket_base64Key.GetValue(other.peerSocket));
            // TODO: Check if this works
            //return peerSocket == other.peerSocket;
        }
    }
}
