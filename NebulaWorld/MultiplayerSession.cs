using NebulaAPI;
using NebulaModel;
using NebulaModel.Logger;
using NebulaWorld.Factory;
using NebulaWorld.GameDataHistory;
using NebulaWorld.Logistics;
using NebulaWorld.Planet;
using NebulaWorld.Player;
using NebulaWorld.Statistics;
using NebulaWorld.Trash;
using NebulaWorld.Universe;
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
        public ILSShipManager Ships { get; private set; }
        public StationUIManager StationsUI { get; private set; }
        public PlanetManager Planets { get; private set; }
        public StatisticsManager Statistics { get; private set; }
        public TrashManager Trashes { get; private set; }
        public DysonSphereManager DysonSpheres { get; private set; }

        // Some Patch Flags
        public bool IsTankWindowPointerPress { get; set; }
        public bool PlanetRefreshMissingMeshes { get; set; }


        public bool IsGameLoaded { get; set; }

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
            Ships = new ILSShipManager();
            StationsUI = new StationUIManager();
            Planets = new PlanetManager();
            Statistics = new StatisticsManager();
            Trashes = new TrashManager();
            DysonSpheres = new DysonSphereManager();
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
        }

        public void OnGameLoadCompleted()
        {
            if (!IsGameLoaded)
            {
                Log.Info("Game load completed");
                IsGameLoaded = true;

                if (Multiplayer.Session.LocalPlayer.IsInitialDataReceived)
                {
                    Multiplayer.Session.World.SetupInitialPlayerState();
                }
            }
        }
    }
}
