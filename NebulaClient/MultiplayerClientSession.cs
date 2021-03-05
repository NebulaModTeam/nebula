using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using UnityEngine;

namespace NebulaClient
{
    public class MultiplayerClientSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerClientSession Instance { get; protected set; }

        private NetManager client;
        private NebulaConnection serverConnection;

        public NetPacketProcessor PacketProcessor { get; protected set; }
        public bool IsConnected { get; protected set; }
        public bool IsLoadingGame { get; set; }

        private string serverIp;
        private int serverPort;

        private void Awake()
        {
            Instance = this;
        }

        public void Connect(string ip, int port)
        {
            serverIp = ip;
            serverPort = port;

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
            LiteNetLibUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

            client.Start();
            client.Connect(ip, port, "nebula");

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetNetworkProvider(this);
            SimulatedWorld.Initialize();
        }

        void Disconnect()
        {
            IsConnected = false;
            client.Stop();
        }

        public void DestroySession()
        {
            Disconnect();
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered) where T : class, new()
        {
            serverConnection?.SendPacket(packet, deliveryMethod);
        }

        public void Reconnect()
        {
            SimulatedWorld.Clear();
            Disconnect();
            Connect(serverIp, serverPort);
        }

        private void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            PacketProcessor.ReadAllPackets(reader, new NebulaConnection(peer, PacketProcessor));
        }

        private void OnPeerConnected(NetPeer peer)
        {
            Log.Info($"Server connection established: {peer.EndPoint}");
            serverConnection = new NebulaConnection(peer, PacketProcessor);
            IsConnected = true;
            SendPacket(new HandshakeRequest());
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            IsConnected = false;
            serverConnection = null;

            InGamePopup.ShowWarning(
                "Connection Lost",
                $"You have been disconnect of the server.\nReason{disconnectInfo.Reason}",
                "Quit", "Reconnect",
                () => { LocalPlayer.LeaveGame(); },
                () => { Reconnect(); });
        }

        private void Update()
        {
            client?.PollEvents();

            // The first 10 ticks are used while loading
            if (IsLoadingGame && GameMain.gameTick > 10)
            {
                IsLoadingGame = false;
                serverConnection?.SendPacket(new SyncComplete());
                InGamePopup.FadeOut();
            }
        }
    }
}
