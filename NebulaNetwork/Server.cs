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
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using UnityEngine;
using Valve.Sockets;

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

        private readonly int port;
        private readonly bool loadSaveFile;
        private uint listenSocket;
        private uint pollGroup;

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

            pollGroup = sockets.CreatePollGroup();

            var processor = PacketProcessor;
            var manager = PlayerManager;

            StatusCallback status = (ref StatusInfo info) =>
            {
                switch (info.connectionInfo.state)
                {
                    case ConnectionState.None:
                        break;

                    case ConnectionState.Connecting:
                        if (Multiplayer.Session.IsGameLoaded == false)
                        {
                            sockets.CloseConnection(info.connection, (int)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.", true);
                        }
                        else
                        {
                            sockets.AcceptConnection(info.connection);
                            sockets.SetConnectionPollGroup(pollGroup, info.connection);
                        }                        
                        break;

                    case ConnectionState.Connected:
                        ((Server)Multiplayer.Session.Network).OnOpen(ref info, processor, manager);
                        break;

                    case ConnectionState.ClosedByPeer:
                    case ConnectionState.ProblemDetectedLocally:
                        sockets.CloseConnection(info.connection);
                        ((Server)Multiplayer.Session.Network).OnClose(ref info, processor, manager);
                        break;
                }
            };

            Address address = new Address();
            address.SetAddress("::0", (ushort)port);

            listenSocket = sockets.CreateListenSocket(ref address);

            utils.SetStatusCallback(status);

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = true;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                Config.Options.GetMechaColors(),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName), loadSaveFile);

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        public override void Stop()
        {
            //TODO: forcibly close all connections

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

            sockets.Poll(0);
            sockets.RunCallbacks();

            NetworkingMessage[] messages = new NetworkingMessage[100];

            var count = sockets.ReceiveMessagesOnPollGroup(pollGroup, messages, 100);
            for(int i = 0; i < count; ++i)
            {
                OnMessage(messages[i]);
            }

            PacketProcessor.ProcessPacketQueue();
        }

        protected void OnOpen(ref StatusInfo info, NetPacketProcessor processor, IPlayerManager manager)
        {
            //NebulaModel.Logger.Log.Info($"Client connected ID: {info.connection}");
            //EndPoint endPoint = new IPEndPoint(IPAddress.Parse(info.connectionInfo.address.GetIP()), info.connectionInfo.address.port);
            //NebulaConnection conn = new NebulaConnection(sockets, info.connection, endPoint, processor);
            //manager.PlayerConnected(conn);
        }

        protected void OnMessage(NetworkingMessage message)
        {
            ConnectionInfo info = new ConnectionInfo();
            sockets.GetConnectionInfo(message.connection, ref info);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(info.address.GetIP()), info.address.port);

            byte[] rawData = new byte[message.length];
            message.CopyTo(rawData);

            PacketProcessor.EnqueuePacketForProcessing(rawData, new NebulaConnection(sockets, message.connection, endPoint, PacketProcessor));
        }

        protected void OnClose(ref StatusInfo info, NetPacketProcessor processor, IPlayerManager manager)
        {
            // If the reason of a client disconnect is because we are still loading the game,
            // we don't need to inform the other clients since the disconnected client never
            // joined the game in the first place.
            if (info.connectionInfo.endReason == (int)DisconnectionReason.HostStillLoading)
            {
                return;
            }

            var connection = info.connection;
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(info.connectionInfo.address.GetIP()), info.connectionInfo.address.port);

            NebulaModel.Logger.Log.Info($"Client disconnected: {info.connection}, reason: {info.connectionInfo.endDebug}");
            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // This is to make sure that we don't try to deal with player disconnection
                // if it is because we have stopped the server and are not in a multiplayer game anymore.
                if (Multiplayer.IsActive)
                {
                    manager.PlayerDisconnected(new NebulaConnection(sockets, connection, endPoint, processor));
                }
            });
        }
    }
}
