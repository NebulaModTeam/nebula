#region

using System;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GalacticTransport))]
internal class GalacticTransport_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GalacticTransport.SetForNewGame))]
    public static void SetForNewGame_Postfix()
    {
        if (Multiplayer.IsActive && !Multiplayer.Session.LocalPlayer.IsHost)
        {
            Multiplayer.Session.Network.SendPacket(new ILSRequestgStationPoolSync());
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.AddStationComponent))]
    public static bool AddStationComponent_Prefix(StationComponent station)
    {
        return !Multiplayer.IsActive || !Multiplayer.Session.LocalPlayer.IsClient || station.gid != 0;
        // When client build a new station (gid == 0), we will let host decide the value of gid
        // ILS will be added when ILSAddStationComponent from host arrived
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.RemoveStationComponent))]
    public static bool RemoveStationComponent_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost || Multiplayer.Session.Ships.PatchLockILS;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.GameTick))]
    public static bool GameTick_Prefix()
    {
        return !Multiplayer.IsActive || Multiplayer.Session.LocalPlayer.IsHost;
        // Let host determine when ships will dispatch. Client will send out ships once receiving ILSIdleShipBackToWork packet
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.AddStation2StationRoute))]
    public static void AddStation2StationRoute_Prefix(int gid0, int gid1)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        Multiplayer.Session.Network.SendPacket(new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.AddStation2StationRoute, gid0, gid1));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.RemoveStation2StationRoute), new Type[] { typeof(int) })]
    public static void RemoveStation2StationRoute_Single_Prefix(int gid)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        Multiplayer.Session.Network.SendPacket(new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.RemoveStation2StationRoute_Single, gid));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.RemoveStation2StationRoute), new Type[] { typeof(int), typeof(int) })]
    public static void RemoveStation2StationRoute_Pair_Prefix(int gid0, int gid1)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        Multiplayer.Session.Network.SendPacket(new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.RemoveStation2StationRoute_Pair, gid0, gid1));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.AddAstro2AstroRoute))]
    public static void AddAstro2AstroRoute_Prefix(int astroId0, int astroId1, int itemId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        Multiplayer.Session.Network.SendPacket(new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.AddAstro2AstroRoute, astroId0, astroId1, itemId));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.RemoveAstro2AstroRoute))]
    public static void RemoveAstro2AstroRoute_Prefix(int astroId0, int astroId1, int itemId)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        Multiplayer.Session.Network.SendPacket(new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.RemoveAstro2AstroRoute, astroId0, astroId1, itemId));
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.SetAstro2AstroRouteEnable))]
    public static void SetAstro2AstroRouteEnable_Prefix(int astroId0, int astroId1, int itemId, bool enable)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        var packet = new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.SetAstro2AstroRouteEnable, astroId0, astroId1, itemId)
        {
            Enable = enable
        };
        Multiplayer.Session.Network.SendPacket(packet);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalacticTransport.SetAstro2AstroRouteComment))]
    public static void SetAstro2AstroRouteComment_Prefix(int astroId0, int astroId1, int itemId, string comment)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS) return;

        var packet = new ILSUpdateRoute(ILSUpdateRoute.ERouteEvent.SetAstro2AstroRouteComment, astroId0, astroId1, itemId)
        {
            Comment = comment
        };
        Multiplayer.Session.Network.SendPacket(packet);
    }
}
