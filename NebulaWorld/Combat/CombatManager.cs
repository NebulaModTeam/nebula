#region

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Combat.Mecha;
using NebulaModel.Packets.Players;
using UnityEngine;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Combat;

public class CombatManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public static int PlayerId { get; private set; }
    public static bool SerializeOverwrite { get; set; }

    public struct PlayerPosition
    {
        public ushort id;
        public int planetId;
        public int starId;
        public Vector3 position;
        public VectorLF3 uPosition;
        public bool isAlive;
        public Mecha mecha;
        public Vector3 skillTargetL;
        public VectorLF3 skillTargetULast;
        public VectorLF3 skillTargetU;
    }

    public PlayerPosition[] Players; // include self
    public HashSet<int> ActivedPlanets;
    public HashSet<int> ActivedStars;
    public HashSet<int> ActivedStarsMechaInSpace; // player in the system and not on a planet
    public Dictionary<int, int> IndexByPlayerId;

    private PlayerAction_Combat actionCombat;
    private static CombatManager instance;

    public CombatManager()
    {
        Players = new PlayerPosition[2];
        ActivedPlanets = [];
        ActivedStars = [];
        ActivedStarsMechaInSpace = [];
        IndexByPlayerId = [];
        instance = this;
    }

    public void Dispose()
    {
        PlayerId = 1;
        actionCombat = null;
        Players = null;
        ActivedPlanets = null;
        ActivedStars = null;
        ActivedStarsMechaInSpace = null;
        IndexByPlayerId = null;
        instance = null;
        GC.SuppressFinalize(this);
    }

    public void GameTick()
    {
        if (!Multiplayer.Session.IsGameLoaded) return;
        var gameTick = GameMain.gameTick;
        ActivedPlanets.Clear();
        ActivedStars.Clear();
        ActivedStarsMechaInSpace.Clear();
        IndexByPlayerId.Clear();
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            // Mimic SkillSystem.CollectPlayerStates()
            if (Players.Length != (remotePlayersModels.Count + 1))
            {
                Players = new PlayerPosition[remotePlayersModels.Count + 1];
            }
            PlayerId = Multiplayer.Session.LocalPlayer.Id;
            Players[0].id = Multiplayer.Session.LocalPlayer.Id;
            Players[0].planetId = GameMain.localPlanet?.id ?? -1;
            Players[0].starId = GameMain.localStar?.id ?? -1;
            Players[0].position = GameMain.mainPlayer.position;
            Players[0].uPosition = GameMain.mainPlayer.uPosition;
            Players[0].isAlive = GameMain.mainPlayer.isAlive;
            var macha = GameMain.mainPlayer.mecha;
            Players[0].mecha = macha;
            Players[0].skillTargetL = macha.skillTargetLCenter;
            Players[0].skillTargetULast = Players[0].skillTargetU;
            Players[0].skillTargetU = macha.skillTargetUCenter;

            ActivedPlanets.Add(Players[0].planetId);
            ActivedStars.Add(Players[0].starId);
            if (Players[0].planetId <= 0 && Players[0].starId > 0)
            {
                ActivedStarsMechaInSpace.Add(Players[0].starId);
            }
            IndexByPlayerId[Players[0].id] = 0;

            var localPlanetId = Players[0].planetId;
            var index = 1;
            foreach (var pair in remotePlayersModels)
            {
                var snapshot = pair.Value.Movement.GetLastPosition();
                var planetData = GameMain.galaxy.PlanetById(snapshot.LocalPlanetId);
                var player = pair.Value.PlayerInstance;
                if (planetData != null) // On planet
                {
                    player.uPosition = planetData.uPosition + (VectorLF3)(planetData.runtimeRotation * player.position);
                    player.uRotation = planetData.runtimeRotation * Maths.SphericalRotation(player.position, 0f);
                }
                else // In space
                {
                    player.uPosition = pair.Value.Movement.absolutePosition;
                }
                ref var ptr = ref Players[index];
                ptr.id = pair.Key;
                ptr.planetId = snapshot.LocalPlanetId;
                ptr.starId = pair.Value.Movement.LocalStarId;
                // If the remote player is on the same planet, player.position is more precise
                // Otherwise it has to use the interpolated recevied position
                ptr.position = ptr.id == localPlanetId ? player.position : snapshot.LocalPlanetPosition.ToVector3();
                ptr.uPosition = player.uPosition;
                ptr.isAlive = player.isAlive;

                macha = pair.Value.MechaInstance;
                ptr.mecha = macha;
                ptr.skillTargetL = macha.skillTargetLCenter;
                ptr.skillTargetULast = ptr.skillTargetU;
                ptr.skillTargetU = macha.skillTargetUCenter;
                macha.energyShieldEnergy = macha.energyShieldEnergyRate > 1 ? 0 : int.MaxValue;

                ActivedPlanets.Add(ptr.planetId);
                ActivedPlanets.Add(ptr.starId);
                if (ptr.planetId <= 0 && ptr.starId > 0)
                {
                    ActivedStarsMechaInSpace.Add(ptr.starId);
                }
                IndexByPlayerId[pair.Key] = index;
                ++index;

                player.controller.actionDeath.GameTick(gameTick);
            }
        }
    }

    public bool ShieldBrust(MechaShieldBurstPacket packet)
    {
        if (actionCombat == null)
        {
            actionCombat = new PlayerAction_Combat();
            actionCombat.Init(GameMain.mainPlayer);
        }

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(packet.PlayerId, out var playerModel))
            {
                return false;
            }
            PlayerId = packet.PlayerId;

            // CollectStates
            actionCombat.localPlanet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
            actionCombat.localStar = GameMain.galaxy.StarById(playerModel.Movement.LocalStarId);
            actionCombat.localFactory = actionCombat.localPlanet?.factory;
            actionCombat.localAstroId = actionCombat.localPlanet?.astroId ?? 0;

            actionCombat.player = playerModel.PlayerInstance;
            actionCombat.mecha = playerModel.MechaInstance;
            actionCombat.localPlayerPos = actionCombat.localPlanet != null ? actionCombat.player.position : actionCombat.player.uPosition;

            var mecha = actionCombat.mecha;
            mecha.energyShieldBurstProgress = packet.EnergyShieldBurstProgress;
            mecha.energyShieldCapacity = packet.EnergyShieldCapacity;
            mecha.energyShieldEnergy = packet.EnergyShieldEnergy;
            mecha.energyShieldBurstDamageRate = packet.EnergyShieldBurstDamageRate;

            var localPlanetOrStarAstroId = actionCombat.skillSystem.localPlanetOrStarAstroId;
            actionCombat.skillSystem.localPlanetOrStarAstroId = actionCombat.localAstroId;
            actionCombat.ShieldBurst();

            // Restore
            actionCombat.skillSystem.localPlanetOrStarAstroId = localPlanetOrStarAstroId;
            PlayerId = Multiplayer.Session.LocalPlayer.Id;
        }
        return true;
    }

    public bool ShootTarget(ushort playerId, int ammoItemId, EAmmoType ammoType, int targetAstroId, int targetId)
    {
        if (actionCombat == null)
        {
            actionCombat = new PlayerAction_Combat();
            actionCombat.Init(GameMain.mainPlayer);
        }

        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (!remotePlayersModels.TryGetValue(playerId, out var playerModel))
            {
                return false;
            }
            PlayerId = playerId;

            // CollectStates
            actionCombat.localPlanet = GameMain.galaxy.PlanetById(playerModel.Movement.localPlanetId);
            actionCombat.localStar = GameMain.galaxy.StarById(playerModel.Movement.LocalStarId);
            actionCombat.localFactory = actionCombat.localPlanet?.factory;
            actionCombat.localAstroId = actionCombat.localPlanet?.astroId ?? 0;

            actionCombat.player = playerModel.PlayerInstance;
            actionCombat.mecha = playerModel.MechaInstance;
            actionCombat.mecha.laserEnergy = int.MaxValue;
            actionCombat.mecha.ammoItemId = ammoItemId;

            var isLocal = targetAstroId == actionCombat.localAstroId;
            var pool = isLocal ? actionCombat.localFactory?.enemyPool : actionCombat.spaceSector.enemyPool;
            if (pool == null || targetId >= pool.Length)
            {
                return false;
            }
            var target = new SkillTarget
            {
                id = targetId,
                astroId = targetAstroId
            };

            actionCombat.ShootTarget(ammoType, target);
            PlayerId = Multiplayer.Session.LocalPlayer.Id;
        }
        return true;
    }

    public void OnFactoryLoadFinished(PlanetFactory factory)
    {
        var cursor = factory.defenseSystem.turrets.cursor;
        var buffer = factory.defenseSystem.turrets.buffer;
        for (var id = 1; id < cursor; id++)
        {
            if (buffer[id].id == id)
            {
                //Remove turretLaserContinuous
                buffer[id].projectileId = 0;
            }
        }
    }

    public void OnAstroFactoryUnload()
    {
        //Remove all projectiles
        GameMain.data.spaceSector.skillSystem.SetForNewGame();
    }
}
