#region

using System;
using System.Collections.Generic;
using NebulaAPI.DataStructures;
using NebulaModel.DataStructures;
using NebulaModel.Packets.Players;
using UnityEngine;
using Random = UnityEngine.Random;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Player;

public class GizmoManager : IDisposable
{
    private ushort indicatorPlayerId;
    private int indicatorPlanetId;
    private Vector3 indicatorPos;
    private LineGizmo naviIndicatorGizmo;
    private LineGizmo naviIndicatorGizmoStarmap;

    public void Dispose()
    {
        naviIndicatorGizmo = null;
        naviIndicatorGizmoStarmap = null;
        GC.SuppressFinalize(this);
    }

    public void SetIndicatorPlayerId(ushort playerId)
    {
        indicatorPlayerId = playerId;
        GameMain.mainPlayer.navigation.indicatorAstroId = 0;
        GameMain.mainPlayer.navigation.indicatorMsgId = 0;
        GameMain.mainPlayer.navigation.indicatorEnemyId = 0;
    }

    public void SetIndicatorPing(int planetId, Vector3 pos)
    {
        indicatorPlanetId = planetId;
        indicatorPos = pos;
        GameMain.mainPlayer.navigation.indicatorAstroId = 0;
        GameMain.mainPlayer.navigation.indicatorMsgId = 0;
        GameMain.mainPlayer.navigation.indicatorEnemyId = 0;
    }

    public void OnUpdate()
    {
        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        if (GameMain.mainPlayer.gizmo.naviIndicatorGizmo != null
            && naviIndicatorGizmo != null) // When main player is navigating to other target, close the exisitng one
        {
            indicatorPlayerId = 0;
            indicatorPlanetId = 0;
        }

        if (indicatorPlayerId != 0 && Multiplayer.Session.Combat.IndexByPlayerId.TryGetValue(indicatorPlayerId, out var index))
        {
            ref var ptr = ref Multiplayer.Session.Combat.Players[index];
            UpdateNavigationGizmo(ptr.planetId, ptr.skillTargetL, ptr.skillTargetU);
            return;
        }

        var planet = GameMain.galaxy.PlanetById(indicatorPlanetId);
        if (indicatorPlanetId != 0 && planet != null)
        {
            UpdateNavigationGizmo(indicatorPlanetId, indicatorPos, planet.uPosition);
            return;
        }

        // Close gizmos if there is no vaild target
        indicatorPlayerId = 0;
        indicatorPlanetId = 0;
        if (naviIndicatorGizmoStarmap != null)
        {
            naviIndicatorGizmoStarmap.gameObject.layer = 0;
            naviIndicatorGizmoStarmap.Close();
            naviIndicatorGizmoStarmap = null;
        }
        if (naviIndicatorGizmo != null)
        {
            naviIndicatorGizmo.Close();
            naviIndicatorGizmo = null;
        }
    }

    private void UpdateNavigationGizmo(int planetId, in Vector3 lpos, in VectorLF3 upos)
    {
        var gizmo = GameMain.mainPlayer.gizmo;
        if (naviIndicatorGizmo == null)
        {
            naviIndicatorGizmo = LineGizmo.Create(1, gizmo.player.position, lpos);
            naviIndicatorGizmo.autoRefresh = true;
            naviIndicatorGizmo.multiplier = 1.5f;
            naviIndicatorGizmo.alphaMultiplier = 0.6f;
            naviIndicatorGizmo.width = 1.8f;
            naviIndicatorGizmo.color = Configs.builtin.gizmoColors[4];
            naviIndicatorGizmo.Open();
        }
        if (naviIndicatorGizmoStarmap == null)
        {
            naviIndicatorGizmoStarmap = LineGizmo.Create(1, gizmo.player.position, lpos);
            naviIndicatorGizmoStarmap.autoRefresh = true;
            naviIndicatorGizmoStarmap.multiplier = 1.5f;
            naviIndicatorGizmoStarmap.alphaMultiplier = 0.3f;
            naviIndicatorGizmoStarmap.width = 0.01f;
            naviIndicatorGizmoStarmap.color = Configs.builtin.gizmoColors[4];
            naviIndicatorGizmoStarmap.Open();
        }

        if (gizmo.player.planetId == planetId)
        {
            naviIndicatorGizmo.spherical = true;
            naviIndicatorGizmo.startPoint = gizmo.player.mecha.skillTargetLCenter;
            naviIndicatorGizmo.endPoint = lpos;
        }
        else
        {
            naviIndicatorGizmo.spherical = false;
            naviIndicatorGizmo.startPoint = gizmo.player.position;
            naviIndicatorGizmo.endPoint = lpos;
        }

        var starmap = UIRoot.instance.uiGame.starmap;
        naviIndicatorGizmoStarmap.startPoint = (gizmo.player.uPosition - starmap.viewTargetUPos) * 0.00025;
        naviIndicatorGizmoStarmap.endPoint = (upos - starmap.viewTargetUPos) * 0.00025;
        naviIndicatorGizmoStarmap.gameObject.layer = 20;
    }
}
