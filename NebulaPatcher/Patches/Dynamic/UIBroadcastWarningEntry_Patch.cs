#region

using HarmonyLib;
using NebulaModel.Packets.Factory.Tank;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIBroadcastWarningEntry))]
internal class UIBroadcastWarningEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIBroadcastWarningEntry.DrawLine))]
    public static bool DrawLine_Prefix(UIBroadcastWarningEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        // Fix guide line to unloaded factory in client
        switch (__instance.broadcastData.vocal)
        {
            case EBroadcastVocal.LandingRelay:
            case EBroadcastVocal.ApproachingSeed:
                // Guard to avoid index out of range exception
                if (__instance.broadcastData.context >= __instance.window.gameData.spaceSector.enemyPool.Length)
                {
                    return false;
                }
                return true;

            case EBroadcastVocal.BuildingDestroyed:
            case EBroadcastVocal.MineralDepleted:
            case EBroadcastVocal.OilSeepDepleted:
                // Replace factory reference with planet
                var position = __instance.trackerTrans.transform.position;
                var vector = UIRoot.instance.overlayCanvas.worldCamera.WorldToScreenPoint(position);
                var startPos = GameCamera.main.ScreenPointToRay(vector).GetPoint(4.5f);
                var relativePos = __instance.window.gameData.relativePos;
                var relativeRot = __instance.window.gameData.relativeRot;
                var planet = GameMain.galaxy.PlanetById(__instance.broadcastData.astroId);
                if (planet == null) return true;

                var lpos = __instance.broadcastData.lpos;
                var vectorLF5 = planet.uPosition + Maths.QRotateLF(planet.runtimeRotation, lpos);
                var vectorLF6 = Maths.QInvRotateLF(relativeRot, vectorLF5 - relativePos);
                UniverseSimulator.VirtualMapping(vectorLF6.x, vectorLF6.y, vectorLF6.z, GameCamera.main.transform.position, out var endPos, out _, 10000.0);

                if (__instance.gizmo == null)
                {
                    __instance.gizmo = LineGizmo.Create(1, startPos, endPos);
                    __instance.gizmo.autoRefresh = false;
                    __instance.gizmo.multiplier = 1.5f;
                    __instance.gizmo.alphaMultiplier = 0.4f;
                    __instance.gizmo.width = 0.15f;
                    __instance.gizmo.color = Configs.builtin.gizmoColors[3];
                    __instance.gizmo.spherical = false;
                    __instance.gizmo.Open();
                    return false;
                }
                __instance.gizmo.startPoint = startPos;
                __instance.gizmo.endPoint = endPos;
                __instance.gizmo.ManualRefresh();
                return false;

            default:
                return true;
        }
    }
}
