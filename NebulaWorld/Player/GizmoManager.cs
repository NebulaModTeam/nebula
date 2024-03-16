#region

using System;
using System.Text;
using NebulaWorld.Chat.ChatLinks;
using NebulaWorld.MonoBehaviours.Local.Chat;
using UnityEngine;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaWorld.Player;

public class GizmoManager : IDisposable
{
    public ushort ObservingPlayerId { get; set; }
    public int ObservingPlanetId { get; set; }
    public Vector3 ObservingPos { get; set; }

    private ushort indicatorPlayerId;
    private int indicatorPlanetId;
    private Vector3 indicatorPos;
    private LineGizmo naviIndicatorGizmo;
    private LineGizmo naviIndicatorGizmoStarmap;
    private CircleGizmo targetGizmo0;
    private CircleGizmo targetGizmo1;

    public void Dispose()
    {
        naviIndicatorGizmo = null;
        naviIndicatorGizmoStarmap = null;
        targetGizmo0 = null;
        targetGizmo1 = null;
        GC.SuppressFinalize(this);
    }

    public void SetIndicatorPlayerId(ushort playerId)
    {
        CloseTargetGizmos();
        if (indicatorPlayerId == playerId)
        {
            indicatorPlayerId = 0;
            return;
        }
        indicatorPlayerId = playerId;

        indicatorPlanetId = 0;
        GameMain.mainPlayer.navigation.indicatorAstroId = 0;
        GameMain.mainPlayer.navigation.indicatorMsgId = 0;
        GameMain.mainPlayer.navigation.indicatorEnemyId = 0;
    }

    public void SetIndicatorPing(int planetId, Vector3 pos)
    {
        CloseTargetGizmos();
        if (indicatorPlanetId == planetId)
        {
            indicatorPlanetId = 0;
            return;
        }
        indicatorPlanetId = planetId;
        indicatorPos = pos;

        indicatorPlayerId = 0;
        GameMain.mainPlayer.navigation.indicatorAstroId = 0;
        GameMain.mainPlayer.navigation.indicatorMsgId = 0;
        GameMain.mainPlayer.navigation.indicatorEnemyId = 0;
    }

    public void OnUpdate()
    {
        if (VFInput.alt && VFInput.control && Input.GetMouseButtonDown(0)) GetMapPing();
        UpdateIndicator();
    }

    private static void GetMapPing()
    {
        // Modify from UIGlobemap.TeleportLogic
        var mainCam = Camera.main;
        if (mainCam == null || GameMain.localPlanet == null) return;
        if (!Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out var hitInfo, 800f, 8720, QueryTriggerInteraction.Collide)) return;

        var starmap = UIRoot.instance.uiGame.starmap;
        if (starmap.active)
        {
            // In starmap view, get the focusing planet or star (OnCursorFunction3Click)
            int astroId;
            string displayString;
            if (starmap.focusPlanet != null)
            {
                astroId = starmap.focusPlanet.planet.id;
                displayString = starmap.focusPlanet.planet.displayName;
            }
            else if (starmap.focusStar != null)
            {
                astroId = starmap.focusStar.star.astroId;
                displayString = starmap.focusStar.star.displayName;
            }
            else if (starmap.focusHive != null)
            {
                astroId = starmap.focusHive.hive.hiveAstroId;
                displayString = starmap.focusHive.hive.displayName;
            }
            else
            {
                return;
            }
            ChatManager.Instance.InsetTextToChatbox(NavigateChatLinkHandler.FormatNavigateToAstro(astroId, displayString), false);
            return;
        }

        Maths.GetLatitudeLongitude(hitInfo.point, out var latd, out _, out var logd, out _,
            out var north, out _, out _, out var east);

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(GameMain.localPlanet.displayName);
        stringBuilder.Append(' ');
        stringBuilder.Append(north == true ? 'N' : 'S');
        stringBuilder.Append(latd);
        stringBuilder.Append('°');
        stringBuilder.Append(east == true ? 'E' : 'W');
        stringBuilder.Append(logd);
        stringBuilder.Append('°');

        var str = NavigateChatLinkHandler.FormatNavigateToPlanetPos(GameMain.localPlanet.id, hitInfo.point, stringBuilder.ToString());
        ChatManager.Instance.InsetTextToChatbox(str, false);
    }

    private void UpdateIndicator()
    {
        if (GameMain.mainPlayer.gizmo.naviIndicatorGizmo != null
            && naviIndicatorGizmo != null) // When main player is navigating to other target, close the existing one
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
            if (planet == GameMain.localPlanet)
            {
                UpdateNavigationGizmo(indicatorPlanetId, indicatorPos, planet.uPosition);
                CreateTargetGizmos(indicatorPos);
            }
            else
            {
                // Convert position on planet to world position
                var pos = Maths.QInvRotateLF(GameMain.data.relativeRot, planet.uPosition - GameMain.data.relativePos);
                UpdateNavigationGizmo(indicatorPlanetId, pos + (VectorLF3)indicatorPos, planet.uPosition);
                CloseTargetGizmos();
            }
            return;
        }

        // Close gizmos if there is no valid target
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
        CloseTargetGizmos();
    }

    private void UpdateNavigationGizmo(int planetId, in Vector3 lpos, in VectorLF3 upos)
    {
        var gizmo = GameMain.mainPlayer.gizmo;
        if (naviIndicatorGizmo == null)
        {
            naviIndicatorGizmo = LineGizmo.Create(1, gizmo.player.mecha.skillTargetLCenter, lpos);
            naviIndicatorGizmo.autoRefresh = true;
            naviIndicatorGizmo.multiplier = 1.5f;
            naviIndicatorGizmo.alphaMultiplier = 1.0f;
            naviIndicatorGizmo.width = 1.8f;
            naviIndicatorGizmo.color = Configs.builtin.gizmoColors[4];
            naviIndicatorGizmo.Open();
        }
        if (naviIndicatorGizmoStarmap == null)
        {
            naviIndicatorGizmoStarmap = LineGizmo.Create(1, gizmo.player.position, lpos);
            naviIndicatorGizmoStarmap.autoRefresh = true;
            naviIndicatorGizmoStarmap.multiplier = 1.5f;
            naviIndicatorGizmoStarmap.alphaMultiplier = 0.6f;
            naviIndicatorGizmoStarmap.width = 0.03f;
            naviIndicatorGizmoStarmap.color = Configs.builtin.gizmoColors[4];
            naviIndicatorGizmoStarmap.spherical = false;
            naviIndicatorGizmoStarmap.Open();
        }

        if (planetId > 0 && gizmo.player.planetId == planetId)
        {
            naviIndicatorGizmo.spherical = true;
            naviIndicatorGizmo.startPoint = gizmo.player.mecha.skillTargetLCenter;
            naviIndicatorGizmo.endPoint = lpos;
            if (targetGizmo0 != null)
            {
                // In planet view, enlarge the target circle
                targetGizmo0.radius = GameCamera.instance.planetMode ? 10f : 1f;
            }
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

    private void CreateTargetGizmos(Vector3 pos)
    {
        if (targetGizmo0 == null)
        {
            targetGizmo0 = CircleGizmo.Create(2, pos, 1f);
            targetGizmo0.multiplier = 2f;
            targetGizmo0.alphaMultiplier = 1.0f;
            targetGizmo0.fadeInScale = 1.3f;
            targetGizmo0.fadeInTime = 0.13f;
            targetGizmo0.fadeInFalloff = 0.5f;
            targetGizmo0.color = Configs.builtin.gizmoColors[2];
            targetGizmo0.rotateSpeed = 60f;
            targetGizmo0.Open();
        }

        if (targetGizmo1 == null)
        {
            targetGizmo1 = CircleGizmo.Create(4, pos, 3f);
            targetGizmo1.multiplier = 2f;
            targetGizmo1.alphaMultiplier = 0.5f;
            targetGizmo1.fadeInScale = 1.3f;
            targetGizmo1.fadeInTime = 0.13f;
            targetGizmo1.fadeInFalloff = 0.5f;
            targetGizmo1.color = Configs.builtin.gizmoColors[2];
            targetGizmo1.rotateSpeed = 60f;
            targetGizmo1.Open();
        }
    }

    private void CloseTargetGizmos()
    {
        if (targetGizmo0 != null)
        {
            targetGizmo0.Close();
            targetGizmo0 = null;
        }
        if (targetGizmo1 != null)
        {
            targetGizmo1.Close();
            targetGizmo1 = null;
        }
    }
}
