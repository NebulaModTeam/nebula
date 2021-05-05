using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.Statistics;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NebulaHost
{
    public class MultiplayerHostSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerHostSession Instance { get; protected set; }

        private WebSocketServer socketServer;

        public PlayerManager PlayerManager { get; protected set; }
        public NetPacketProcessor PacketProcessor { get; protected set; }
        public StatisticsManager StatisticsManager { get; protected set; }

        float gameStateUpdateTimer = 0;
        float gameResearchHashUpdateTimer = 0;
        float productionStatisticsUpdateTimer = 0;


        const float GAME_STATE_UPDATE_INTERVAL = 1;
        const float GAME_RESEARCH_UPDATE_INTERVAL = 2;
        const float STATISTICS_UPDATE_INTERVAL = 1;

        private void Awake()
        {
            Instance = this;
        }

        public void StartServer(int port, bool loadSaveFile = false)
        {
            PlayerManager = new PlayerManager();
            if (loadSaveFile)
            {
                SaveManager.LoadServerData();
            }
            PacketProcessor = new NetPacketProcessor();
            StatisticsManager = new StatisticsManager();

#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor);

            socketServer = new WebSocketServer(port);
            socketServer.AddWebSocketService("/socket", () => new WebSocketService(PlayerManager, PacketProcessor));

            socketServer.Start();

            SimulatedWorld.Initialize();

            LocalPlayer.SetNetworkProvider(this);
            LocalPlayer.IsMasterClient = true;

            // TODO: Load saved player info here
            LocalPlayer.SetPlayerData(new PlayerData(PlayerManager.GetNextAvailablePlayerId(), GameMain.localPlanet?.id ?? -1, new Float3(1.0f, 0.6846404f, 0.243137181f), AccountData.me.userName));
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

        public void SendPacketToLocalStar<T>(T packet) where T : class, new()
        {
            PlayerManager.SendPacketToLocalStar(packet);
        }

        public void SendPacketToLocalPlanet<T>(T packet) where T : class, new()
        {
            PlayerManager.SendPacketToLocalPlanet(packet);
        }

        public void SendPacketToPlanet<T>(T packet, int planetId) where T : class, new()
        {
            PlayerManager.SendPacketToPlanet(packet, planetId);
        }

        public void SendPacketToStar<T>(T packet, int starId) where T : class, new()
        {
            PlayerManager.SendPacketToStar(packet, starId);
        }

        private void Update()
        {
            gameStateUpdateTimer += Time.deltaTime;
            gameResearchHashUpdateTimer += Time.deltaTime;
            productionStatisticsUpdateTimer += Time.deltaTime;

            if (gameStateUpdateTimer > GAME_STATE_UPDATE_INTERVAL)
            {
                gameStateUpdateTimer = 0;
                SendPacket(new GameStateUpdate() { State = new GameState(TimeUtils.CurrentUnixTimestampMilliseconds(), GameMain.gameTick) });
            }

            if (gameResearchHashUpdateTimer > GAME_RESEARCH_UPDATE_INTERVAL)
            {
                gameResearchHashUpdateTimer = 0;
                if (GameMain.data.history.currentTech != 0)
                {
                    TechState state = GameMain.data.history.techStates[GameMain.data.history.currentTech];
                    SendPacket(new GameHistoryResearchUpdatePacket(GameMain.data.history.currentTech, state.hashUploaded, state.hashNeeded));
                }
            }

            if (productionStatisticsUpdateTimer > STATISTICS_UPDATE_INTERVAL)
            {
                productionStatisticsUpdateTimer = 0;
                StatisticsManager.SendBroadcastIfNeeded();
            }

            PacketProcessor.ProcessPacketQueue();
        }

        private class WebSocketService : WebSocketBehavior
        {
            private readonly PlayerManager playerManager;
            private readonly NetPacketProcessor packetProcessor;

            public WebSocketService(PlayerManager playerManager, NetPacketProcessor packetProcessor)
            {
                this.playerManager = playerManager;
                this.packetProcessor = packetProcessor;
            }

            protected override void OnOpen()
            {
                if (SimulatedWorld.IsGameLoaded == false)
                {
                    // Reject any connection that occurs while the host's game is loading.
                    this.Context.WebSocket.Close((ushort)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.");
                    return;
                }

                NebulaModel.Logger.Log.Info($"Client connected ID: {ID}");
                NebulaConnection conn = new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor);
                playerManager.PlayerConnected(conn);
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                packetProcessor.EnqueuePacketForProcessing(e.RawData, new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
            }

            protected override void OnClose(CloseEventArgs e)
            {
                // If the reason of a client disonnect is because we are still loading the game,
                // we don't need to inform the other clients since the disconnected client never
                // joined the game in the first place.
                if (e.Code == (short)DisconnectionReason.HostStillLoading)
                    return;

                NebulaModel.Logger.Log.Info($"Client disconnected: {ID}, reason: {e.Reason}");
                UnityDispatchQueue.RunOnMainThread(() =>
                {
                    playerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
                });
            }

            protected override void OnError(ErrorEventArgs e)
            {
                // TODO: Decide what to do here - does OnClose get called too?
            }
        }
    }
}
