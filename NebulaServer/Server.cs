using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NebulaServer
{
    public class Server : INetEventListener
    {
        private readonly NetManager server;

        Dictionary<ushort, NebulaConnection> clients;

        public NetPacketProcessor PacketProcessor { get; }

        public Server()
        {
            server = new NetManager(this)
            {
                AutoRecycle = true,
            };
            clients = new Dictionary<ushort, NebulaConnection>();
            PacketProcessor = new NetPacketProcessor();
            PacketProcessor.RegisterNestedType<NebulaId>();
            PacketProcessor.RegisterNestedType<Float3>();
            PacketProcessor.RegisterNestedType<Float4>();
            PacketProcessor.RegisterNestedType<NebulaTransform>();
            PacketProcessor.RegisterNestedType<NebulaAnimationState>();
            PacketProcessor.SubscribeReusable<Movement, NebulaConnection> (OnPlayerMovement);
            PacketProcessor.SubscribeReusable<PlayerAnimationUpdate, NebulaConnection> (OnPlayerAnimationUpdate);
            PacketProcessor.SubscribeReusable<PlayerSpawned, NebulaConnection> (OnPlayerSpawned);
        }

        public void Start(int port)
        {
            server.Start(port);
        }

        public void Stop()
        {
            server.Stop();
        }

        public void Update()
        {
            server?.PollEvents();
        }

        public void SendPacketToAll<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            server.SendToAll(PacketProcessor.Write(packet), deliveryMethod);
        }

        public void SendPacketToOthers<T>(ushort excludedClientId, T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            foreach(var client in clients)
            {
                if (client.Key != excludedClientId)
                {
                    client.Value.SendPacket(packet, deliveryMethod);
                }
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            request.AcceptIfKey("nebula");
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketProcessor.ReadAllPackets(reader, new NebulaConnection(peer, PacketProcessor));
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"Client connected: {peer.EndPoint}");
            NebulaConnection clientConn = new NebulaConnection(peer, PacketProcessor);
            clientConn.SendPacket(new PlayerJoinedSession((ushort)peer.Id));
            clients.Add((ushort)peer.Id, clientConn);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Client disconnected: {peer.EndPoint}, reason: {disconnectInfo.Reason}");
            clients.Remove((ushort)peer.Id);
        }

        private void OnPlayerMovement(Movement packet, NebulaConnection conn)
        {
            packet.PlayerId = conn.Id;
            SendPacketToOthers(conn.Id, packet, DeliveryMethod.Unreliable);
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            packet.PlayerId = conn.Id;
            SendPacketToOthers(conn.Id, packet, DeliveryMethod.Unreliable);
        }

        private void OnPlayerSpawned(PlayerSpawned packet, NebulaConnection conn)
        {
            packet.PlayerId = conn.Id;
            SendPacketToOthers(conn.Id, packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
