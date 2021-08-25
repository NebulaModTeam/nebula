using HarmonyLib;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;
using NebulaModel.Packets.Session;
using NebulaWorld.Factory;
using NebulaWorld.GameDataHistory;
using NebulaWorld.Logistics;
using NebulaWorld.MonoBehaviours;
using NebulaWorld.MonoBehaviours.Local;
using NebulaWorld.Planet;
using NebulaWorld.Player;
using NebulaWorld.Statistics;
using NebulaWorld.Trash;
using NebulaWorld.Universe;
using UnityEngine;
using System;

namespace NebulaWorld
{
    public class MultiplayerSession : IDisposable
    {
        public NetworkProvider Network { get; private set; }
        public LocalPlayer LocalPlayer { get; private set; }
        public SimulatedWorld World { get; private set; }
        public FactoryManager Factories { get; private set; }
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


        // TODO: Move this in the MultiplayerSession maybe ?
        public void OnGameLoadCompleted()
        {
            Log.Info("Game has finished loading");

            // Assign our own color
            World.UpdatePlayerColor(Multiplayer.Session.LocalPlayer.Id, LocalPlayer.Data.MechaColor);

            // Change player location from spawn to the last known
            VectorLF3 uPosition = new VectorLF3(LocalPlayer.Data.UPosition.x, LocalPlayer.Data.UPosition.y, LocalPlayer.Data.UPosition.z);
            if (uPosition != VectorLF3.zero)
            {
                GameMain.mainPlayer.planetId = LocalPlayer.Data.LocalPlanetId;
                if (LocalPlayer.Data.LocalPlanetId == -1)
                {
                    GameMain.mainPlayer.uPosition = uPosition;
                }
                else
                {
                    GameMain.mainPlayer.position = LocalPlayer.Data.LocalPlanetPosition.ToVector3();
                    GameMain.mainPlayer.uPosition = new VectorLF3(GameMain.localPlanet.uPosition.x + GameMain.mainPlayer.position.x, GameMain.localPlanet.uPosition.y + GameMain.mainPlayer.position.y, GameMain.localPlanet.uPosition.z + GameMain.mainPlayer.position.z);
                }
                GameMain.mainPlayer.uRotation = Quaternion.Euler(LocalPlayer.Data.Rotation.ToVector3());

                //Load player's saved data from the last session.
                AccessTools.Property(typeof(global::Player), "package").SetValue(GameMain.mainPlayer, LocalPlayer.Data.Mecha.Inventory, null);
                GameMain.mainPlayer.mecha.forge = LocalPlayer.Data.Mecha.Forge;
                GameMain.mainPlayer.mecha.coreEnergy = LocalPlayer.Data.Mecha.CoreEnergy;
                GameMain.mainPlayer.mecha.reactorEnergy = LocalPlayer.Data.Mecha.ReactorEnergy;
                GameMain.mainPlayer.mecha.reactorStorage = LocalPlayer.Data.Mecha.ReactorStorage;
                GameMain.mainPlayer.mecha.warpStorage = LocalPlayer.Data.Mecha.WarpStorage;
                GameMain.mainPlayer.SetSandCount(LocalPlayer.Data.Mecha.SandCount);

                //Fix references that brokes during import
                AccessTools.Property(typeof(MechaForge), "mecha").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer.mecha, null);
                AccessTools.Property(typeof(MechaForge), "player").SetValue(GameMain.mainPlayer.mecha.forge, GameMain.mainPlayer, null);
                GameMain.mainPlayer.mecha.forge.gameHistory = GameMain.data.history;
            }

            //Initialization on the host side after game is loaded
            Multiplayer.Session.Factories.InitializePrebuildRequests();

            if (!Multiplayer.Session.LocalPlayer.IsHost)
            {
                // Update player's Mecha tech bonuses
                LocalPlayer.Data.Mecha.TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);

                // Enable Ping Indicator for Clients
                World.DisplayPingIndicator();

                // Notify the server that we are done loading the game
                Network.SendPacket(new SyncComplete());

                // Subscribe for the local star events
                Network.SendPacket(new PlayerUpdateLocalStarId(GameMain.data.localStar.id));

                // Hide the "Joining Game" popup
                InGamePopup.FadeOut();
            }

            // Finally we need add the local player components to the player character
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerMovement>();
            GameMain.mainPlayer.gameObject.AddComponentIfMissing<LocalPlayerAnimation>();

            IsGameLoaded = true;
        }

    }
}
