using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using System;

namespace NebulaClient
{
    public class Client
    {
        private readonly NetManager client;
        private NebulaConnection serverConnection;

        public bool IsConnected { get; protected set; }
        public bool IsSessionJoined { get; protected set; }
        public ushort LocalPlayerId { get; protected set; }

        public NetPacketProcessor PacketProcessor { get; }

        public Client()
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += OnPeerConnected;
            listener.PeerDisconnectedEvent += OnPeerDisconnected;
            listener.NetworkReceiveEvent += OnNetworkReceive;

            client = new NetManager(listener)
            {
                AutoRecycle = true,
            };

            PacketProcessor = new NetPacketProcessor();
            PacketProcessor.RegisterNestedType<NebulaId>();
            PacketProcessor.RegisterNestedType<Float3>();
            PacketProcessor.RegisterNestedType<Float4>();
            PacketProcessor.RegisterNestedType<NebulaTransform>();
            PacketProcessor.RegisterNestedType<NebulaAnimationState>();
            PacketProcessor.SubscribeReusable<PlayerJoinedSession>(OnSessionJoined);
        }

        public void Connect(string ip, int port)
        {
            client.Start();
            client.Connect(ip, port, "nebula");
        }

        public void Disconnect()
        {
            IsConnected = false;
            IsSessionJoined = false;
            client.Stop();
        }

        public void Update()
        {
            client?.PollEvents();
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            if (serverConnection != null)
            {
                serverConnection.SendPacket(packet, deliveryMethod);
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketProcessor.ReadAllPackets(reader, new NebulaConnection(peer, PacketProcessor));
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Console.WriteLine("Connected to server");
            serverConnection = new NebulaConnection(peer, PacketProcessor);
            IsConnected = true;
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Disconnected from server: {disconnectInfo.Reason}");
            serverConnection = null;
            IsConnected = false;
            IsSessionJoined = false;
        }

        private void OnSessionJoined(PlayerJoinedSession packet)
        {
            LocalPlayerId = packet.Id;
            IsSessionJoined = true;
            Console.WriteLine($"Received my LocalPlayerId: {LocalPlayerId}");
        }
    }
}
