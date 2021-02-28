using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaServer.GameLogic;
using System;
using System.Net;
using System.Net.Sockets;

namespace NebulaServer
{
    public class Server : INetEventListener
    {
        private readonly NetManager server;

        PlayerManager playerManager;

        public NetPacketProcessor PacketProcessor { get; }

        public Server()
        {
            server = new NetManager(this)
            {
                AutoRecycle = true,
            };

            playerManager = new PlayerManager();

            PacketProcessor = new NetPacketProcessor();
            PacketProcessor.RegisterNestedType<NebulaId>();
            PacketProcessor.RegisterNestedType<Float3>();
            PacketProcessor.RegisterNestedType<Float4>();
            PacketProcessor.RegisterNestedType<NebulaTransform>();
            PacketProcessor.RegisterNestedType<NebulaAnimationState>();
            PacketProcessor.SubscribeReusable<Movement, NebulaConnection> (OnPlayerMovement);
            PacketProcessor.SubscribeReusable<PlayerAnimationUpdate, NebulaConnection> (OnPlayerAnimationUpdate);
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
            playerManager.PlayerConnected(clientConn);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Console.WriteLine($"Client disconnected: {peer.EndPoint}, reason: {disconnectInfo.Reason}");
            playerManager.PlayerDisconnected(new NebulaConnection(peer, PacketProcessor));
        }

        private void OnPlayerMovement(Movement packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            packet.PlayerId = player.Id;
            player.UpdatePosition(packet);
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);
        }

        private void OnPlayerAnimationUpdate(PlayerAnimationUpdate packet, NebulaConnection conn)
        {
            Player player = playerManager.GetPlayer(conn);
            packet.PlayerId = player.Id;
            playerManager.SendPacketToOtherPlayers(packet, player, DeliveryMethod.Unreliable);
        }
    }
}
