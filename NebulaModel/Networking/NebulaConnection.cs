using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Net;

namespace NebulaModel.Networking
{
    public class NebulaConnection
    {
        private readonly NetPeer peer;
        private readonly NetPacketProcessor packetProcessor;

        public ushort Id => (ushort)peer.Id;
        public int Ping => peer.Ping;
        public IPEndPoint Endpoint => peer.EndPoint;

        public NebulaConnection(NetPeer peer, NetPacketProcessor packetProcessor)
        {
            this.peer = peer;
            this.packetProcessor = packetProcessor;
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (peer.ConnectionState == ConnectionState.Connected)
            {
                peer.Send(packetProcessor.Write(packet), deliveryMethod);
                peer.Flush();
            }
            else
            {
                Console.WriteLine($"Cannot send packet {packet?.GetType()} to a closed connection {peer?.EndPoint}");
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
            return Equals((NebulaConnection)obj);
        }

        public override int GetHashCode()
        {
            return peer?.Id.GetHashCode() ?? 0;
        }

        protected bool Equals(NebulaConnection other)
        {
            return peer?.Id == other.peer?.Id;
        }
    }
}
