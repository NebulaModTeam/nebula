using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Threading;

namespace NebulaClient.NetworkingLayer.LiteNetLib
{
    public class LiteNetLibClient
    {
        public bool IsConnected { get; private set; }

        private readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        private readonly AutoResetEvent connectedEvent = new AutoResetEvent(false);
        private NetManager client;

        public LiteNetLibClient()
        {

        }

        public void Start(string ipAddress, int serverPort)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            listener.PeerConnectedEvent += Connected;
            listener.PeerDisconnectedEvent += Disconnected;
            listener.NetworkReceiveEvent += ReceivedNetworkData;

            client = new NetManager(listener);
            client.Start();
            client.Connect(ipAddress, serverPort, "nebula");

            connectedEvent.WaitOne(2000);
            connectedEvent.Reset();
        }

        public void Send()
        {
            //client.SendToAll(netPacketProcessor.Write(0), DeliveryMethod.ReliableOrdered);
        }

        public void Stop()
        {
            IsConnected = false;
            client.Stop();
        }

        private void Connected(NetPeer peer)
        {
            connectedEvent.Set();
            IsConnected = true;
        }

        private void Disconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            IsConnected = false;
            // TODO: Show LostConnection modal in game
        }

        private void ReceivedNetworkData(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }
    }
}
