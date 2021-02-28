using LiteNetLib;
using LiteNetLib.Utils;
using NebulaClient.MonoBehaviours;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Utils;
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
            LiteNetLibUtils.RegisterAllPacketNestedTypes(PacketProcessor);

            PacketProcessor.SubscribeReusable<JoinSessionConfirmed>(OnJoinSessionConfirmed);
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

            UIMessageBox.Show(
                "Connection Lost",
                $"You have been disconnect of the server.\nReason{disconnectInfo.Reason}",
                "Quit",
                "Reconnect",
                0,
                new UIMessageBox.Response(() => {
                    MultiplayerSession.instance.LeaveGame();
                }),
                new UIMessageBox.Response(() => {
                    MultiplayerSession.instance.TryToReconnect();
                }));
        }

        private void OnJoinSessionConfirmed(JoinSessionConfirmed packet)
        {
            LocalPlayerId = packet.LocalPlayerId;
            IsSessionJoined = true;
            Console.WriteLine($"Client PlayerId is: {LocalPlayerId}");
        }
    }
}
