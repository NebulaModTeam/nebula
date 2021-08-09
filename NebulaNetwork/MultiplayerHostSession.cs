using BepInEx;
using Mirror;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Networking;
using NebulaModel.Networking.Serialization;
using NebulaModel.Packets.GameHistory;
using NebulaModel.Packets.GameStates;
using NebulaModel.Utils;
using NebulaWorld;
using NebulaWorld.Statistics;
using System;
using System.Linq;
using UnityEngine;
using static NebulaModel.Networking.NebulaConnection;

namespace NebulaNetwork
{
    public class MultiplayerHostSession : MonoBehaviour, INetworkProvider
    {
        public static MultiplayerHostSession Instance { get; protected set; }
        public PlayerManager PlayerManager { get; protected set; }
        public NetworkManager NetworkManager { get; protected set; }
        public NetPacketProcessor PacketProcessor { get; protected set; }
        public StatisticsManager StatisticsManager { get; protected set; }

        protected NetworkIdentity Identity;

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
            if (loadSaveFile)
            {
                SaveManager.LoadServerData();
            }
            PacketProcessor = new NetPacketProcessor();
            StatisticsManager = new StatisticsManager();

            PlayerManager = new PlayerManager()
            {
                PacketProcessor = PacketProcessor
            };

            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, true);

            NebulaConnection.PacketProcessor = PacketProcessor;

            NetworkManager = MirrorManager.SetupMirror(typeof(HostManager), new UriBuilder("scheme://", "localhost", port, "").Uri);

            NetworkServer.RegisterHandler<NebulaMessage>((networkConnection, nebulaMessage) => OnNebulaMessage(networkConnection, nebulaMessage));
            NetworkServer.RegisterHandler<PacketProcessors.Planet.FactoryLoadRequest>(PacketProcessors.Planet.FactoryLoadRequest.ProcessPacket);
            NetworkServer.RegisterHandler<PacketProcessors.Planet.PlanetDataRequest>(PacketProcessors.Planet.PlanetDataRequest.ProcessPacket);
            NetworkServer.RegisterHandler<PacketProcessors.Universe.DysonSphereLoadRequest>(PacketProcessors.Universe.DysonSphereLoadRequest.ProcessPacket);

            NetworkManager.StartServer();

            LocalPlayer.TryLoadGalacticScale2();

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = true;
            LocalPlayer.SetNetworkProvider(this);

            // TODO: Load saved player info here
            LocalPlayer.SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName));
        }

        private void Update()
        {
            gameStateUpdateTimer += Time.deltaTime;
            gameResearchHashUpdateTimer += Time.deltaTime;
            productionStatisticsUpdateTimer += Time.deltaTime;

            if (gameStateUpdateTimer > GAME_STATE_UPDATE_INTERVAL)
            {
                gameStateUpdateTimer = 0;
                PlayerManager.SendPacketToAllPlayers(new GameStateUpdate() { State = new GameState(TimeUtils.CurrentUnixTimestampMilliseconds(), GameMain.gameTick) });
            }

            if (gameResearchHashUpdateTimer > GAME_RESEARCH_UPDATE_INTERVAL)
            {
                gameResearchHashUpdateTimer = 0;
                if (GameMain.data.history.currentTech != 0)
                {
                    TechState state = GameMain.data.history.techStates[GameMain.data.history.currentTech];
                    PlayerManager.SendPacketToAllPlayers(new GameHistoryResearchUpdatePacket(GameMain.data.history.currentTech, state.hashUploaded, state.hashNeeded));
                }
            }

            if (productionStatisticsUpdateTimer > STATISTICS_UPDATE_INTERVAL)
            {
                productionStatisticsUpdateTimer = 0;
                StatisticsManager.SendBroadcastIfNeeded();
            }

            PacketProcessor.ProcessPacketQueue();
        }

        public void DestroySession()
        {
            NetworkManager.StopServer();
            Destroy(gameObject);
            Destroy(GameObject.Find("Mirror Networking"));
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

        public void SendPacketToStarExclude<T>(T packet, int starId, NetworkConnection exclude) where T : class, new()
        {
            PlayerManager.SendPacketToStarExcept(packet, starId, exclude);
        }
    }

    public class HostManager : NetworkManager
    {
        public override void OnStartServer()
        {
            base.OnStartServer();
        }
        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            if (SimulatedWorld.IsGameLoaded == false)
            {
                // Reject any connection that occurs while the host's game is loading.
                conn.Disconnect();
                return;
            }

            NebulaModel.Logger.Log.Info($"Client connected ID: {conn.connectionId}");
            MultiplayerHostSession.Instance.PlayerManager.PlayerConnected(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            NebulaModel.Logger.Log.Info($"Client disconnected: {conn.connectionId}");
            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                MultiplayerHostSession.Instance.PlayerManager.PlayerDisconnected(conn);
            });
        }
    }
}
