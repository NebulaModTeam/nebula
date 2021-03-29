using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
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
            PlayerManager = new PlayerManager();
            PacketProcessor = new NetPacketProcessor();
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
            LocalPlayer.SetPlayerData(new PlayerData(PlayerManager.GetNextAvailablePlayerId(), GameMain.localPlanet?.id ?? -1, new Float3(1.0f, 0.6846404f, 0.243137181f)));
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
                    this.Context.WebSocket.Close((ushort)NebulaStatusCode.HostStillLoading, "Host still loading, please try again later.");
                    return;
                }

                NebulaModel.Logger.Log.Info($"Client connected ID: {ID}, {Context.UserEndPoint}");
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
                if (e.Code == (short)NebulaStatusCode.HostStillLoading)
                    return;

                NebulaModel.Logger.Log.Info($"Client disconnected: {Context.UserEndPoint}, reason: {e.Reason}");
                playerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
            }

            protected override void OnError(ErrorEventArgs e)
            {
                // TODO: Decide what to do here - does OnClose get called too?
            }
        }
    }
}
