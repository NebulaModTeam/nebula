using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using System;
using System.Reflection;
using UnityEngine;

namespace NebulaNetwork
{
    public class Server : NetworkProvider
    {
        private const float GAME_STATE_UPDATE_INTERVAL = 1;
        private const float GAME_RESEARCH_UPDATE_INTERVAL = 2;
        private const float STATISTICS_UPDATE_INTERVAL = 1;
        private const float LAUNCH_UPDATE_INTERVAL = 2;

        private float gameStateUpdateTimer = 0;
        private float gameResearchHashUpdateTimer = 0;
        private float productionStatisticsUpdateTimer = 0;
        private float dysonLaunchUpateTimer = 0;

        private Telepathy.Server server;

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

            server = new Telepathy.Server(50 * 1024 * 1024)
            {
                OnConnected = OnConnected,
                OnData = OnMessage,
                OnDisconnected = OnDisconnected,
                ReceiveTimeout = 30000,
                SendTimeout = 30000
            };

            server.Start(port);

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = true;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                Config.Options.GetMechaColors(),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName), loadSaveFile);

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        private void OnConnected(int connectionId)
        {
            if (Multiplayer.Session.IsGameLoaded == false)
            {
                // Reject any connection that occurs while the host's game is loading.
                //Context.WebSocket.Close((ushort)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.");
                server.Disconnect(connectionId);
                return;
            }

            NebulaModel.Logger.Log.Info($"Client {connectionId} connected");
            NebulaConnection conn = new NebulaConnection(null, server, PacketProcessor, connectionId);
            Multiplayer.Session.Network.PlayerManager.PlayerConnected(conn);
        }

        private void OnMessage(int connectionId, ArraySegment<byte> message)
        {
            PacketProcessor.EnqueuePacketForProcessing(message.Array, new NebulaConnection(null, server, PacketProcessor, connectionId));
        }

        private void OnDisconnected(int connectionId)
        {
            // If the reason of a client disconnect is because we are still loading the game,
            // we don't need to inform the other clients since the disconnected client never
            // joined the game in the first place.
            //if (e.Code == (short)DisconnectionReason.HostStillLoading)
            //{
            //    return;
            //}

            NebulaModel.Logger.Log.Info($"Client {connectionId} disconnected");
            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // This is to make sure that we don't try to deal with player disconnection
                // if it is because we have stopped the server and are not in a multiplayer game anymore.
                if (Multiplayer.IsActive)
                {
                    Multiplayer.Session.Network.PlayerManager.PlayerDisconnected(new NebulaConnection(null, server, PacketProcessor, connectionId));
                }
            });
        }

        public override void Stop()
        {
            server?.Stop();

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
            server.Tick(10000);
            gameStateUpdateTimer += Time.deltaTime;
            gameResearchHashUpdateTimer += Time.deltaTime;
            productionStatisticsUpdateTimer += Time.deltaTime;
            dysonLaunchUpateTimer += Time.deltaTime;

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

            if (dysonLaunchUpateTimer > LAUNCH_UPDATE_INTERVAL)
            {
                dysonLaunchUpateTimer = 0;
                Multiplayer.Session.Launch.SendBroadcastIfNeeded();
            }

            PacketProcessor.ProcessPacketQueue();
        }
    }
}
