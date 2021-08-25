using HarmonyLib;
using NebulaModel;
using NebulaModel.DataStructures;
using NebulaModel.Logger;
using NebulaWorld.Factory;
using NebulaWorld.GameDataHistory;
using NebulaWorld.Logistics;
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
        public readonly NetworkProvider NetProvider;

        public readonly SimulatedWorld World;

        public readonly FactoryManager Factories;
        public readonly StorageManager Storage;
        public readonly PowerTowerManager PowerTowers;
        public readonly BeltManager Belts;
        public readonly BuildToolManager BuildTools;
        public readonly DroneManager Drones;
        public readonly GameDataHistoryManager History;
        public readonly ILSShipManager Ships;
        public readonly StationUIManager StationsUI;
        public readonly PlanetManager Planets;
        public readonly StatisticsManager Statistics;
        public readonly TrashManager Trashes;
        public readonly DysonSphereManager DysonSpheres;

        public bool IsGameLoaded { get; set; }

        public MultiplayerSession(NetworkProvider networkProvider)
        {
            NetProvider = networkProvider;

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
            NetProvider?.Dispose();
            World?.Dispose();
            Factories?.Dispose();
            Storage?.Dispose();
            PowerTowers?.Dispose();
            Belts?.Dispose();
            BuildTools?.Dispose();
            Drones?.Dispose();
            History?.Dispose();
            Ships?.Dispose();
            StationsUI?.Dispose();
            Planets?.Dispose();
            Statistics?.Dispose();
            Trashes?.Dispose();
            DysonSpheres?.Dispose();
        }


        // TODO: Move this in the MultiplayerSession maybe ?
        public void OnGameLoadCompleted()
        {
            Log.Info("Game has finished loading");

            // Assign our own color
            World.UpdatePlayerColor(LocalPlayer.PlayerId, LocalPlayer.Data.MechaColor);

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

            //Update player's Mecha tech bonuses
            if (!LocalPlayer.IsMasterClient)
            {
                LocalPlayer.Data.Mecha.TechBonuses.UpdateMech(GameMain.mainPlayer.mecha);

                // Enable Ping Indicator for Clients
                // TODO: HG FIX
                // MultiplayerClientSession.Instance.DisplayPingIndicator();
            }

            //Initialization on the host side after game is loaded
            Multiplayer.Session.Factories.InitializePrebuildRequests();

            LocalPlayer.SetReady();

            IsGameLoaded = true;
        }

    }
}
