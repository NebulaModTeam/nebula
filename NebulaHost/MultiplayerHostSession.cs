using LiteNetLib.Utils;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NebulaHost
{
    public class MultiplayerHostSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerHostSession Instance { get; protected set; }

        //private NetManager server;
        private WebSocketServer socketServer;

        public PlayerManager PlayerManager { get; protected set; }
        public NetPacketProcessor PacketProcessor { get; protected set; }

        float gameStateUpdateTimer = 0;

        private void Awake()
        {
            Instance = this;
        }

        public void StartServer(int port)
        {
            /*          EventBasedNetListener listener = new EventBasedNetListener();
                        listener.ConnectionRequestEvent += OnConnectionRequest;
                        listener.PeerConnectedEvent += OnPeerConnected;
                        listener.PeerDisconnectedEvent += OnPeerDisconnected;
                        listener.NetworkReceiveEvent += OnNetworkReceive;

                        server = new NetManager(listener)
                        {
                            AutoRecycle = true,
            #if DEBUG
                            SimulateLatency = true,
                            SimulatePacketLoss = true,
                            SimulationMinLatency = 50,
                            SimulationMaxLatency = 100,
            #endif
                        };*/

            PlayerManager = new PlayerManager();
            PacketProcessor = new NetPacketProcessor();
            LiteNetLibUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            LiteNetLibUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

            socketServer = new WebSocketServer(port);
            socketServer.AddWebSocketService("/socket", () => new WebSocketService(PlayerManager, PacketProcessor));

            socketServer.Start();

            SimulatedWorld.Initialize();

            LocalPlayer.SetNetworkProvider(this);
            LocalPlayer.IsMasterClient = true;

            // TODO: Load saved player info here
            LocalPlayer.SetPlayerData(new PlayerData(PlayerManager.GetNextAvailablePlayerId(), new Float3(1.0f, 0.6846404f, 0.243137181f)));
        }

        private void StopServer()
        {
            socketServer?.Stop();
        }

        public void DestroySession()
        {
            StopServer();
            Destroy(gameObject);
        }

        public void SendPacket<T>(T packet) where T : class, new()
        {
            PlayerManager.SendPacketToAllPlayers(packet);
        }

        private void Update()
        {
            gameStateUpdateTimer += Time.deltaTime;
            if (gameStateUpdateTimer > 1)
            {
                SendPacket(new GameStateUpdate() { State = new GameState(TimeUtils.CurrentUnixTimestampMilliseconds(), GameMain.gameTick) });
            }
        }

        private class WebSocketService : WebSocketBehavior
        {
            private PlayerManager playerManager;
            private NetPacketProcessor packetProcessor;

            public WebSocketService(PlayerManager playerManager, NetPacketProcessor packetProcessor)
            {
                this.playerManager = playerManager;
                this.packetProcessor = packetProcessor;
            }

            protected override void OnClose(CloseEventArgs e)
            {
                NebulaModel.Logger.Log.Info($"Client disconnected: {this.Context.UserEndPoint}, reason: {e.Reason}");
                playerManager.PlayerDisconnected(new NebulaConnection(this.Context.WebSocket, packetProcessor));
            }

            protected override void OnError(ErrorEventArgs e)
            {
                base.OnError(e);
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                packetProcessor.ReadPacket(new NetDataReader(e.RawData), new NebulaConnection(this.Context.WebSocket, packetProcessor));
            }

            protected override void OnOpen()
            {
                NebulaModel.Logger.Log.Info($"Client connected ID: {this.ID}, {this.Context.UserEndPoint}");
                NebulaConnection conn = new NebulaConnection(this.Context.WebSocket, packetProcessor);
                playerManager.PlayerConnected(conn);
            }
        }
    }
}
