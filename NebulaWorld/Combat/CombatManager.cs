#region

using System;
using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using UnityEngine;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Combat;

public class CombatManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public static bool LockBuildHp { get; private set; }
    public static int PlayerId { get; private set; }

    public struct PlayerPosition
    {
        public ushort id;
        public int planetId;
        public Vector3 position;
        public VectorLF3 uPosition;
        public Mecha mecha;
        public Vector3 skillTargetL;
        public VectorLF3 skillTargetULast;
        public VectorLF3 skillTargetU;
    }

    public PlayerPosition[] Players; // include self
    public HashSet<int> ActivedPlanets;
    public Dictionary<int, int> IndexByPlayerId;

    private PlayerAction_Combat actionCombat;
    private static CombatManager instance;

    public CombatManager()
    {
        LockBuildHp = true;
        Players = new PlayerPosition[2];
        ActivedPlanets = [];
        IndexByPlayerId = [];
        instance = this;
    }

    public void Dispose()
    {
        LockBuildHp = false;
        PlayerId = 1;
        actionCombat = null;
        Players = null;
        ActivedPlanets = null;
        IndexByPlayerId = null;
        instance = null;
        GC.SuppressFinalize(this);
    }

    public void GameTick()
    {
        if (!Multiplayer.Session.IsGameLoaded)
        {
            return;
        }
        var gameTick = GameMain.gameTick;
        ActivedPlanets.Clear();
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
            Players[0].position = GameMain.mainPlayer.position;
            Players[0].uPosition = GameMain.mainPlayer.uPosition;
            var macha = GameMain.mainPlayer.mecha;
            Players[0].mecha = macha;
            Players[0].skillTargetL = macha.skillTargetLCenter;
            Players[0].skillTargetULast = Players[0].skillTargetU;
            Players[0].skillTargetU = macha.skillTargetUCenter;

            ActivedPlanets.Add(Players[0].planetId);
            IndexByPlayerId[Players[0].id] = 0;
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
                ptr.position = player.position;
                ptr.uPosition = player.uPosition;

                macha = pair.Value.MechaInstance;
                ptr.mecha = macha;
                ptr.skillTargetL = macha.skillTargetLCenter;
                ptr.skillTargetULast = ptr.skillTargetU;
                ptr.skillTargetU = macha.skillTargetUCenter;

                ActivedPlanets.Add(snapshot.LocalPlanetId);
                IndexByPlayerId[pair.Key] = index;
                ++index;

                player.controller.actionDeath.GameTick(gameTick);
            }
        }

        if (Multiplayer.Session.IsClient)
        {
            return;
        }

        // ActivateNearbyEnemyBase
        for (var pid = 0; pid < Players.Length; pid++)
        {
            var planet = GameMain.galaxy.PlanetById(Players[pid].planetId);
            if (planet != null && planet.factoryLoaded)
            {
                var bases = planet.factory.enemySystem.bases;
                var buffer = bases.buffer;
                var enemyPool = planet.factory.enemyPool;
                GameMain.data.spaceSector.InverseTransformToAstro_ref(planet.astroId, ref Players[pid].uPosition, out var vectorLF);
                for (var i = 1; i < bases.cursor; i++)
                {
                    var dfgbaseComponent = buffer[i];
                    if (dfgbaseComponent != null && dfgbaseComponent.id == i && (enemyPool[dfgbaseComponent.enemyId].pos - vectorLF).sqrMagnitude < 8100.0)
                    {
                        dfgbaseComponent.UnderAttack(vectorLF, 50f, 120);
                    }
                }
            }
        }
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
            actionCombat.localStar = actionCombat.localPlanet?.star; // TODO: Assign real star
            actionCombat.localFactory = actionCombat.localPlanet?.factory;
            actionCombat.localAstroId = actionCombat.localPlanet?.astroId ?? 0;

            actionCombat.player = playerModel.PlayerInstance;
            actionCombat.mecha = playerModel.MechaInstance;
            actionCombat.mecha.laserEnergy = int.MaxValue;
            actionCombat.mecha.ammoItemId = ammoItemId;

            var isLocal = targetId == actionCombat.localAstroId;
            var pool = isLocal ? actionCombat.localFactory.enemyPool : actionCombat.spaceSector.enemyPool;
            if (targetId >= pool.Length || pool[targetId].id != targetId)
            {
                NebulaModel.Logger.Log.Debug($"{ammoType} {targetId} doesn't exist!");
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
}
