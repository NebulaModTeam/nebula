#region

using System;
using System.Collections.Generic;
using System.Net.Sockets;
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


    public struct PlayerPosition
    {
        public ushort id;
        public int planetId;
        public Vector3 position;
    }

    public PlayerPosition[] Players; // include self
    public HashSet<int> ActivedPlanets;

    private PlayerAction_Combat actionCombat;

    public CombatManager()
    {
        LockBuildHp = true;
        Players = new PlayerPosition[2];
        ActivedPlanets = [];
    }

    public void Dispose()
    {
        LockBuildHp = false;
        actionCombat = null;
        Players = null;
        ActivedPlanets = null;
        GC.SuppressFinalize(this);
    }

    public void GameTick()
    {
        if (!Multiplayer.Session.IsGameLoaded)
        {
            return;
        }
        ActivedPlanets.Clear();
        using (Multiplayer.Session.World.GetRemotePlayersModels(out var remotePlayersModels))
        {
            if (Players.Length != (remotePlayersModels.Count + 1))
            {
                Players = new PlayerPosition[remotePlayersModels.Count + 1];
            }
            Players[0].id = Multiplayer.Session.LocalPlayer.Id;
            Players[0].planetId = GameMain.localPlanet?.id ?? -1;
            Players[0].position = GameMain.mainPlayer.position;
            ActivedPlanets.Add(Players[0].planetId);
            var index = 1;
            foreach (var pair in remotePlayersModels)
            {
                var snapshot = pair.Value.Movement.GetLastPosition();
                Players[index].id = pair.Key;
                Players[index].planetId = snapshot.LocalPlanetId;
                Players[index].position = snapshot.LocalPlanetPosition.ToVector3();
                ActivedPlanets.Add(snapshot.LocalPlanetId);
                ++index;
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
            if (!remotePlayersModels.TryGetValue(playerId, out var player))
            {
                return false;
            }
            actionCombat.player = player.PlayerInstance;
            actionCombat.mecha = player.MechaInstance;
            actionCombat.mecha.laserEnergy = int.MaxValue;
            actionCombat.mecha.ammoItemId = ammoItemId;
            actionCombat.localFactory = GameMain.localPlanet?.factory;
            actionCombat.localAstroId = GameMain.localPlanet?.astroId ?? 0;

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
        }
        return true;
    }
}
