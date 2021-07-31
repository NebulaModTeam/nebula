using Mirror;
using NebulaModel.DataStructures;
using NebulaModel;
using NebulaWorld;
using System.Linq;
using UnityEngine;
using NebulaModel.Networking.Serialization;
using NebulaWorld.Statistics;
using BepInEx;
using static NebulaModel.Networking.NebulaConnection;
using NebulaModel.Networking;
using kcp2k;

namespace NebulaNetwork
{
    public class MultiplayerHostSession : MonoBehaviour
    {
        public static MultiplayerHostSession Instance { get; protected set; }
        public PlayerManager PlayerManager {  get; protected set; }
        public NetworkManager NetworkManager { get; protected set; }
        public NetPacketProcessor PacketProcessor {  get; protected set; }
        public StatisticsManager StatisticsManager { get; protected set; }

        protected NetworkIdentity Identity;

        private void Awake()
        {
            Instance = this;
        }

        public void StartServer(int port, bool loadSaveFile = false)
        {
            if(loadSaveFile)
            {
                SaveManager.LoadServerData();
            }
            PacketProcessor = new NetPacketProcessor();
            StatisticsManager = new StatisticsManager();
#if DEBUG
            PacketProcessor.SimulateLatency = true;
#endif
            PacketUtils.RegisterAllPacketNestedTypes(PacketProcessor);
            PacketUtils.RegisterAllPacketProcessorsInCallingAssembly(PacketProcessor, true);

            NetworkManager = (NetworkManager)gameObject.AddComponent(typeof(NetworkManager));
            NetworkManager.transport = (TelepathyTransport)gameObject.AddComponent(typeof(TelepathyTransport));
            gameObject.AddComponent(typeof(NetworkManagerHUD));
            NetworkManager.OnValidate();
            Transport.activeTransport = NetworkManager.transport;

            NetworkServer.OnConnectedEvent += OnConnected;
            NetworkServer.OnDisconnectedEvent += OnDisconnected;


            PlayerManager = new PlayerManager() 
            {
                PacketProcessor = PacketProcessor
            };

            NetworkManager.StartServer();

            LocalPlayer.TryLoadGalacticScale2();

            SimulatedWorld.Initialize();

            LocalPlayer.IsMasterClient = true;

            // TODO: Load saved player info here
            LocalPlayer.SetPlayerData(new PlayerData(
                PlayerManager.GetNextAvailablePlayerId(),
                GameMain.localPlanet?.id ?? -1,
                new Float3(Config.Options.MechaColorR / 255, Config.Options.MechaColorG / 255, Config.Options.MechaColorB / 255),
                !string.IsNullOrWhiteSpace(Config.Options.Nickname) ? Config.Options.Nickname : GameMain.data.account.userName));
        }

        public void SetupHost()
        {
            NetworkServer.RegisterHandler<NebulaMessage>(OnNebulaMessage);
        }

        public void OnNebulaMessage(NetworkConnection arg1, NebulaMessage arg2)
        {
            PacketProcessor.EnqueuePacketForProcessing(arg2.Payload.ToArray(), arg1);
        }

        public void OnDisconnected(NetworkConnection obj)
        {
            NebulaModel.Logger.Log.Info($"Client disconnected: {obj.connectionId}");
            ThreadingHelper.Instance.StartSyncInvoke(() =>
            {
                PlayerManager.PlayerDisconnected(obj);
            });
        }

        public void OnConnected(NetworkConnection obj)
        {
            NebulaModel.Logger.Log.Info("Server OnConnected");
            if(SimulatedWorld.IsGameLoaded == false)
            {
                // Reject any connection that occurs while the host's game is loading.
                obj.Disconnect();
                return;
            }

            NebulaModel.Logger.Log.Info($"Client connected ID: {obj.connectionId}");
        }
    }
}
