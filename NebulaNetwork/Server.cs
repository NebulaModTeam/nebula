using NebulaAPI;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using System.Collections.Generic;
using System.Net;
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

        private Dictionary<uint, NebulaConnection> connections = new Dictionary<uint, NebulaConnection>();

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

            pollGroup = Sockets.CreatePollGroup();

            var processor = PacketProcessor;
            var manager = PlayerManager;

            Address address = new Address();
            address.SetAddress("::0", (ushort)port);

            listenSocket = Sockets.CreateListenSocket(ref address);

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost = true;

            ((LocalPlayer)Multiplayer.Session.LocalPlayer).SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                Config.Options.GetMechaColors(),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName), loadSaveFile);

            NebulaModAPI.OnMultiplayerGameStarted?.Invoke();
        }

        protected override void OnEvent(ref StatusInfo info)
        {
            switch (info.connectionInfo.state)
            {
                case ConnectionState.None:
                    break;

                case ConnectionState.Connecting:
                    if (Multiplayer.Session.IsGameLoaded == false)
                    {
                        Sockets.CloseConnection(info.connection, (int)DisconnectionReason.HostStillLoading, "Host still loading, please try again later.", true);
                    }
                    else
                    {
                        Sockets.AcceptConnection(info.connection);
                        Sockets.SetConnectionPollGroup(pollGroup, info.connection);
                    }
                    break;

                case ConnectionState.Connected:
                    OnOpen(ref info);
                    break;

                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    Sockets.CloseConnection(info.connection);
                    OnClose(ref info);
                    break;
            }
        }

        public override void Stop()
        {
            foreach (var kvp in connections)
            {
                Sockets?.CloseConnection(kvp.Key);
            }

            Sockets?.CloseListenSocket(listenSocket);
            Sockets?.DestroyPollGroup(pollGroup);

            NebulaModAPI.OnMultiplayerGameEnded?.Invoke();

            connections.Clear();

            Provider = null;
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

            Sockets.Poll(0);
            Sockets.RunCallbacks();

            NetworkingMessage[] messages = new NetworkingMessage[100];

            var count = Sockets.ReceiveMessagesOnPollGroup(pollGroup, messages, 100);
            for(int i = 0; i < count; ++i)
            {
                OnMessage(messages[i]);
            }

            foreach(var kvp in connections)
            {
                kvp.Value.Update();
            }

            PacketProcessor.ProcessPacketQueue();
        }

        protected void OnOpen(ref StatusInfo info)
        {
            NebulaModel.Logger.Log.Info($"Client connected ID: {info.connection}");
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(info.connectionInfo.address.GetIP()), info.connectionInfo.address.port);
            NebulaConnection conn = new NebulaConnection(Sockets, info.connection, endPoint, PacketProcessor);

            connections.Add(info.connection, conn);

            PlayerManager.PlayerConnected(conn);
        }

        protected void OnMessage(NetworkingMessage message)
        {
            ConnectionInfo info = new ConnectionInfo();
            Sockets.GetConnectionInfo(message.connection, ref info);
            EndPoint endPoint = new IPEndPoint(IPAddress.Parse(info.address.GetIP()), info.address.port);

            NebulaConnection connection;
            if(connections.TryGetValue(message.connection, out connection))
            {
                byte[] rawData = new byte[message.length];
                message.CopyTo(rawData);

                var data = connection.Receive(rawData);
                if(data != null)
                {
                    PacketProcessor.EnqueuePacketForProcessing(data, connection);
                }
            }
        }

        protected void OnClose(ref StatusInfo info)
        {
            NebulaConnection connection = null;
            connections.TryGetValue(info.connection, out connection);

            connections.Remove(info.connection);

            // If the reason of a client disconnect is because we are still loading the game,
            // we don't need to inform the other clients since the disconnected client never
            // joined the game in the first place.
            if (info.connectionInfo.endReason == (int)DisconnectionReason.HostStillLoading)
            {
                return;
            }


            NebulaModel.Logger.Log.Info($"Client disconnected: {info.connection}, reason: {info.connectionInfo.endDebug}");
            UnityDispatchQueue.RunOnMainThread(() =>
            {
                // This is to make sure that we don't try to deal with player disconnection
                // if it is because we have stopped the server and are not in a multiplayer game anymore.
                if (Multiplayer.IsActive && connection != null)
                {
                    PlayerManager.PlayerDisconnected(connection);
                }
            });
        }
    }
}
