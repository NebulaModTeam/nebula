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
        private readonly NetPacketProcessor packetProcessor;

        Dictionary<ushort, NebulaConnection> clients;

        public Server()
        {
            server = new NetManager(this)
            {
                AutoRecycle = true,
            };
            clients = new Dictionary<ushort, NebulaConnection>();
            packetProcessor = new NetPacketProcessor();
            packetProcessor.RegisterNestedType<NebulaId>();
            packetProcessor.RegisterNestedType<Float3>();
            packetProcessor.SubscribeReusable<Movement, NebulaConnection> (OnPlayerMovement);
            packetProcessor.SubscribeReusable<PlayerSpawned, NebulaConnection> (OnPlayerSpawned);
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
            server.SendToAll(packetProcessor.Write(packet), deliveryMethod);
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
            packetProcessor.ReadAllPackets(reader, new NebulaConnection(peer, packetProcessor));
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine($"Client connected: {peer.EndPoint}");
            NebulaConnection clientConn = new NebulaConnection(peer, packetProcessor);
            clients.Add((ushort)peer.Id, clientConn);
            clientConn.SendPacket(new PlayerJoinedSession((ushort)peer.Id));
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

        private void OnPlayerSpawned(PlayerSpawned packet, NebulaConnection conn)
        {
            packet.PlayerId = conn.Id;
            SendPacketToOthers(conn.Id, packet, DeliveryMethod.ReliableOrdered);
        }
    }
}
