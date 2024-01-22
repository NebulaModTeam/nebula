#region

using System;
using System.Net.Sockets;
using NebulaModel.DataStructures;
using UnityEngine;

#endregion

namespace NebulaWorld.Combat;

public class CombatManager : IDisposable
{
    public readonly ToggleSwitch IsIncomingRequest = new();

    public static bool LockBuildHp { get; private set; }

    private PlayerAction_Combat actionCombat;

    public CombatManager()
    {
        LockBuildHp = true;
    }

    public void Dispose()
    {
        LockBuildHp = false;
        actionCombat = null;
        GC.SuppressFinalize(this);
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
