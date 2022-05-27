using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaWorld.Factory;
using NebulaWorld.GameDataHistory;
using NebulaWorld.GameStates;
using NebulaWorld.Logistics;
using NebulaWorld.Planet;
using NebulaWorld.Player;
using NebulaWorld.SocialIntegration;
using NebulaWorld.Statistics;
using NebulaWorld.Trash;
using NebulaWorld.Universe;
using NebulaWorld.Warning;
using System;

namespace NebulaWorld
{
    public class MultiplayerSession : IDisposable, IMultiplayerSession
    {
        public NebulaAPI.INetworkProvider Network { get; private set; }
        public ILocalPlayer LocalPlayer { get; private set; }
        public SimulatedWorld World { get; private set; }
        public IFactoryManager Factories { get; private set; }
        public StorageManager Storage { get; private set; }
        public PowerTowerManager PowerTowers { get; private set; }
        public BeltManager Belts { get; private set; }
        public BuildToolManager BuildTools { get; private set; }
        public DroneManager Drones { get; private set; }
        public GameDataHistoryManager History { get; private set; }
        public GameStatesManager State { get; private set; }
        public ILSShipManager Ships { get; private set; }
        public StationUIManager StationsUI { get; private set; }
        public PlanetManager Planets { get; private set; }
        public StatisticsManager Statistics { get; private set; }
        public TrashManager Trashes { get; private set; }
        public DysonSphereManager DysonSpheres { get; private set; }
        public LaunchManager Launch { get; private set; }
        public WarningManager Warning { get; private set; }

        // Some Patch Flags
        public DateTime StartTime;

        public bool IsGameLoaded { get; set; }
        public bool IsInLobby { get; set; }
        public bool CanPause
        {
            get => canPause;
            set
            {
                canPause = value;
                World?.SetPauseIndicator(value);
            }
        }
        private bool canPause = true;

        public ushort NumPlayers { get; set; } = 1;

        public MultiplayerSession(NetworkProvider networkProvider)
        {
            Network = networkProvider;

            LocalPlayer = new LocalPlayer();
            World = new SimulatedWorld();
            Factories = new FactoryManager();
            Storage = new StorageManager();
            PowerTowers = new PowerTowerManager();
            Belts = new BeltManager();
            BuildTools = new BuildToolManager();
            Drones = new DroneManager();
            History = new GameDataHistoryManager();
            State = new GameStatesManager();
            Ships = new ILSShipManager();
            StationsUI = new StationUIManager();
            Planets = new PlanetManager();
            Statistics = new StatisticsManager();
            Trashes = new TrashManager();
            DysonSpheres = new DysonSphereManager();
            Launch = new LaunchManager();
            Warning = new WarningManager();
            
            StartTime = DateTime.Now;
        }

        public void Dispose()
        {
            Network?.Dispose();
            Network = null;

            LocalPlayer?.Dispose();
            LocalPlayer = null;

            World?.Dispose();
            World = null;

            Factories?.Dispose();
            Factories = null;

            Storage?.Dispose();
            Storage = null;

            PowerTowers?.Dispose();
            PowerTowers = null;

            Belts?.Dispose();
            Belts = null;

            BuildTools?.Dispose();
            BuildTools = null;

            Drones?.Dispose();
            Drones = null;

            History?.Dispose();
            History = null;

            State?.Dispose();
            State = null;

            Ships?.Dispose();
            Ships = null;

            StationsUI?.Dispose();
            StationsUI = null;

            Planets?.Dispose();
            Planets = null;

            Statistics?.Dispose();
            Statistics = null;

            Trashes?.Dispose();
            Trashes = null;

            DysonSpheres?.Dispose();
            DysonSpheres = null;

            Launch?.Dispose();
            Launch = null;

            Warning?.Dispose();
            Warning = null;
        }

        public void OnGameLoadCompleted()
        {
            if (!IsGameLoaded)
            {
                Log.Info("Game load completed");
                IsGameLoaded = true;
                DiscordManager.UpdateRichPresence();
                ((NebulaModel.NetworkProvider)Multiplayer.Session.Network).PacketProcessor.Enable = true;
                Log.Info($"OnGameLoadCompleted: Resume PacketProcessor");

                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    GameMain.history.universeObserveLevel = SimulatedWorld.GetUniverseObserveLevel();
                }

                if (Multiplayer.Session.LocalPlayer.IsInitialDataReceived)
                {
                    Multiplayer.Session.World.SetupInitialPlayerState();
                }
            }
        }
    }
}
