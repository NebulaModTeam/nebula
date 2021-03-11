using LiteNetLib;
using LiteNetLib.Utils;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Session;
using NebulaModel.Utils;
using NebulaWorld;
using UnityEngine;
using WebSocketSharp;

namespace NebulaClient
{
    public class MultiplayerClientSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerClientSession Instance { get; protected set; }

        private WebSocket clientSocket;
        private NebulaConnection serverConnection;

        public NetPacketProcessor PacketProcessor { get; protected set; }
        public bool IsConnected { get; protected set; }

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

            /*            EventBasedNetListener listener = new EventBasedNetListener();
                        listener.PeerConnectedEvent += OnPeerConnected;
                        listener.PeerDisconnectedEvent += OnPeerDisconnected;
                        listener.NetworkReceiveEvent += OnNetworkReceive;

                        client = new NetManager(listener)
                        {
                            AutoRecycle = true,
                        };*/

            clientSocket = new WebSocket($"ws://{ip}:{port}/socket");
            clientSocket.OnOpen += ClientSocket_OnOpen;
            clientSocket.OnClose += ClientSocket_OnClose;
            clientSocket.OnMessage += ClientSocket_OnMessage;

            PacketProcessor = new NetPacketProcessor();
            LiteNetLibUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            LiteNetLibUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

/*            client.Start();
            client.Connect(ip, port, "nebula");*/

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = false;
            LocalPlayer.SetNetworkProvider(this);
        }

        void Disconnect()
        {
            IsConnected = false;
            clientSocket.Close();
        }

        public void DestroySession()
        {
            Disconnect();
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            serverConnection?.SendPacket(packet);
        }

        public void Reconnect()
        {
            SimulatedWorld.Clear();
            Disconnect();
            Connect(serverIp, serverPort);
        }

        private void ClientSocket_OnMessage(object sender, MessageEventArgs e)
        {
            PacketProcessor.ReadPacket(new NetDataReader(e.RawData), new NebulaConnection(clientSocket, PacketProcessor));
        }

        private void ClientSocket_OnOpen(object sender, System.EventArgs e)
        {
            Log.Info($"Server connection established: {clientSocket.Url}");
            serverConnection = new NebulaConnection(clientSocket, PacketProcessor);
            IsConnected = true;
            SendPacket(new HandshakeRequest());
        }

        private void ClientSocket_OnClose(object sender, CloseEventArgs e)
        {
            IsConnected = false;
            serverConnection = null;

            InGamePopup.ShowWarning(
                "Connection Lost",
                $"You have been disconnect of the server.\nReason{e.Reason}",
                "Quit", "Reconnect",
                () => { LocalPlayer.LeaveGame(); },
                () => { Reconnect(); });
        }

        private void Update()
        {
            //client?.PollEvents();
        }
    }
}
