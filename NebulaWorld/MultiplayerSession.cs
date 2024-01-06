﻿#region

using System;
using NebulaAPI.GameState;
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

#endregion

namespace NebulaWorld;

public class MultiplayerSession : IDisposable, IMultiplayerSession
{
    private bool canPause = true;

    // Some Patch Flags
    public DateTime StartTime;

    public MultiplayerSession(NebulaAPI.GameState.INetworkProvider networkProvider)
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
        Couriers = new CourierManager();
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

    public SimulatedWorld World { get; set; }
    public StorageManager Storage { get; set; }
    public PowerTowerManager PowerTowers { get; set; }
    public BeltManager Belts { get; set; }
    public BuildToolManager BuildTools { get; set; }
    private DroneManager Drones { get; set; }
    public GameDataHistoryManager History { get; set; }
    private GameStatesManager State { get; set; }
    public CourierManager Couriers { get; set; }
    public ILSShipManager Ships { get; set; }
    public StationUIManager StationsUI { get; set; }
    public PlanetManager Planets { get; set; }
    public StatisticsManager Statistics { get; set; }
    public TrashManager Trashes { get; set; }
    public DysonSphereManager DysonSpheres { get; set; }
    public LaunchManager Launch { get; set; }
    public WarningManager Warning { get; set; }
    public bool IsInLobby { get; set; }

    public bool CanPause
    {
        get => canPause;
        set
        {
            canPause = value;
            SimulatedWorld.SetPauseIndicator(value);
        }
    }

    public ushort NumPlayers { get; set; } = 1;

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

        Couriers?.Dispose();
        Couriers = null;

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

        GC.SuppressFinalize(this);
    }

    public NebulaAPI.GameState.INetworkProvider Network { get; set; }
    public ILocalPlayer LocalPlayer { get; set; }
    public IFactoryManager Factories { get; set; }

    public bool IsGameLoaded { get; set; }

    public void OnGameLoadCompleted()
    {
        if (IsGameLoaded)
        {
            return;
        }
        Log.Info("Game load completed");
        IsGameLoaded = true;
        DiscordManager.UpdateRichPresence();
        Multiplayer.Session.Network.PacketProcessor.EnablePacketProcessing = true;
        Log.Info("OnGameLoadCompleted: Resume PacketProcessor");

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
