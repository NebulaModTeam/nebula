using HarmonyLib;
using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NebulaNetwork
{
    public class Server : NetworkProvider
    {
        private const float GAME_STATE_UPDATE_INTERVAL = 1;
        private const float GAME_RESEARCH_UPDATE_INTERVAL = 2;
        private const float STATISTICS_UPDATE_INTERVAL = 1;

        private float gameStateUpdateTimer = 0;
        private float gameResearchHashUpdateTimer = 0;
        private float productionStatisticsUpdateTimer = 0;

        private WebSocketServer socket;

        private readonly int port;
        private readonly bool loadSaveFile;

        public Server(int port, bool loadSaveFile = false) : base(new PlayerManager())
        {
            this.port = port;
            this.loadSaveFile = loadSaveFile;
        }

        public override void Start()
        {
            if (loadSaveFile)
            {
                SaveManager.LoadServerData();
            }

            foreach (Assembly assembly in AssembliesUtils.GetNebulaAssemblies())
            {
                PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor);
            }
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, true);

            foreach (Assembly assembly in NebulaModAPI.TargetAssemblies)
            {
                PacketUtils.RegisterAllPacketNestedTypesInAssembly(assembly, PacketProcessor);
                PacketUtils.RegisterAllPacketProcessorsInAssembly(assembly, PacketProcessor, true);
            }
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif

            socket = new WebSocketServer(System.Net.IPAddress.IPv6Any, port);
            DisableNagleAlgorithm(socket);
            socket.AddWebSocketService("/socket", () => new WebSocketService(PlayerManager, PacketProcessor));
            socket.Start();

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = true;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName), loadSaveFile);

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        public override void Stop()
        {
            socket?.Stop();

            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void SendPacket<T>(T packet)
        {
            PlayerManager.SendPacketToAllPlayers(packet);
        }

        public override void SendPacketToLocalStar<T>(T packet)
        {
            PlayerManager.SendPacketToLocalStar(packet);
        }

        public override void SendPacketToLocalPlanet<T>(T packet)
        {
            PlayerManager.SendPacketToLocalPlanet(packet);
        }

        public override void SendPacketToPlanet<T>(T packet, int planetId)
        {
            PlayerManager.SendPacketToPlanet(packet, planetId);
        }

        public override void SendPacketToStar<T>(T packet, int starId)
        {
            PlayerManager.SendPacketToStar(packet, starId);
        }

        public override void SendPacketToStarExclude<T>(T packet, int starId, INebulaConnection exclude)
        {
            PlayerManager.SendPacketToStarExcept(packet, starId, (NebulaConnection)exclude);
        }

        public override void Update()
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
                Multiplayer.Session.Statistics.SendBroadcastIfNeeded();
            }

            PacketProcessor.ProcessPacketQueue();
        }

        private void DisableNagleAlgorithm(WebSocketServer socketServer)
        {
            TcpListener listener = AccessTools.FieldRefAccess<WebSocketServer, TcpListener>("_listener")(socketServer);
            listener.Server.NoDelay = true;
        }

        private class WebSocketService : WebSocketBehavior
        {
            private readonly IPlayerManager playerManager;
            private readonly NetPacketProcessor packetProcessor;

            public WebSocketService(IPlayerManager playerManager, NetPacketProcessor packetProcessor)
            {
                this.playerManager = playerManager;
                this.packetProcessor = packetProcessor;
            }

            protected override void OnOpen()
            {
                if (Multiplayer.Session.IsGameLoaded == false)
                {
                    // Reject any connection that occurs while the host's game is loading.
                    Context.WebSocket.Close((ushort)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.");
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
                // If the reason of a client disconnect is because we are still loading the game,
                // we don't need to inform the other clients since the disconnected client never
                // joined the game in the first place.
                if (e.Code == (short)DisconnectionReason.HostStillLoading)
                {
                    return;
                }

                NebulaModel.Logger.Log.Info($"Client disconnected: {ID}, reason: {e.Reason}");
                UnityDispatchQueue.RunOnMainThread(() =>
                {
                    // This is to make sure that we don't try to deal with player disconnection
                    // if it is because we have stopped the server and are not in a multiplayer game anymore.
                    if (Multiplayer.IsActive)
                    {
                        playerManager.PlayerDisconnected(new NebulaConnection(Context.WebSocket, Context.UserEndPoint, packetProcessor));
                    }
                });
            }

            protected override void OnError(ErrorEventArgs e)
            {
                // TODO: Decide what to do here - does OnClose get called too?
            }
        }
    }
}
