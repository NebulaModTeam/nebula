#region

using System;
using System.Collections.Generic;
using HarmonyLib;
using NebulaModel.Packets.Warning;
using NebulaWorld;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(WarningSystem))]
internal class WarningSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.RemoveWarningData))]
    [HarmonyPatch(nameof(WarningSystem.WarningLogic))]
    public static bool AlterWarningData_Prefix()
    {
        //Let warningPool only be updated by packet
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.NewWarningData))]
    public static void NewWarningData_Prefix(WarningSystem __instance, ref int factoryId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }

        // Stop the code to access unreachable factory
        factoryId = -1;
        // Let it return a dummy WarningData pool[0]
        __instance.warningRecycleCursor = 1;
        __instance.warningRecycle[0] = 0;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.CalcFocusDetail))]
    public static void CalcFocusDetail_Prefix(int __0)
    {
        if (__0 == 0 || !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost)
        {
            return;
        }
        if (Multiplayer.Session.Warning.TickSignal == Multiplayer.Session.Warning.TickData)
        {
            return;
        }
        if (GameMain.gameTick - Multiplayer.Session.Warning.LastRequestTime <= 240)
        {
            return;
        }
        Multiplayer.Session.Network.SendPacket(new WarningDataRequest(WarningRequestEvent.Data));
        Multiplayer.Session.Warning.LastRequestTime = GameMain.gameTick;
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.CheckRelayAndSeed))]
    [HarmonyPatch(nameof(WarningSystem.CheckShieldCollapse))]
    public static bool DisableCheckInClient()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.IsServer;
    }

    static readonly HashSet<EBroadcastVocal> syncBroadcasts =
    [
        EBroadcastVocal.LandingRelay,
        EBroadcastVocal.PlanetaryShieldDown,
        EBroadcastVocal.ApproachingSeed,
        EBroadcastVocal.BuildingDestroyed,
        EBroadcastVocal.MineralDepleted,
        EBroadcastVocal.OilSeepDepleted
    ];

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.Broadcast), new Type[] { typeof(EBroadcastVocal), typeof(int), typeof(int), typeof(int), typeof(Vector3) })]
    public static bool Broadcast_Prefix(WarningSystem __instance, EBroadcastVocal vocal, int factoryIndex, int astroId, int context, Vector3 lpos)
    {
        if (!Multiplayer.IsActive || !syncBroadcasts.Contains(vocal)) return true;

        // In client, let server authorize the trigger of those sync broadcast
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Warning.IsIncomingBroadcast.Value;
        lock (__instance.broadcasts)
        {
            if (__instance.IndexOf(vocal, factoryIndex, astroId, context) < 0) // new data
            {
                Multiplayer.Session.Server.SendPacket(new WarningBroadcastDataPacket(vocal, astroId, context, lpos));
            }
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WarningSystem.Broadcast), new Type[] { typeof(EBroadcastVocal), typeof(int), typeof(int), typeof(int) })]
    public static bool Broadcast_Prefix(WarningSystem __instance, EBroadcastVocal vocal, int factoryIndex, int astroId, int context)
    {
        if (!Multiplayer.IsActive || !syncBroadcasts.Contains(vocal)) return true;

        // In client, let server authorize the trigger of those sync broadcast
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Warning.IsIncomingBroadcast.Value;
        lock (__instance.broadcasts)
        {
            if (__instance.IndexOf(vocal, factoryIndex, astroId, context) < 0) // new data
            {
                Multiplayer.Session.Server.SendPacket(new WarningBroadcastDataPacket(vocal, astroId, context, Vector3.zero));
            }
        }
        return true;
    }
}
