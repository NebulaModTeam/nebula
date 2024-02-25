#region

using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Combat.DFRelay;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(DFRelayComponent))]
internal class DFRelayComponent_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.SearchTargetPlaceProcess))]
    public static bool SearchTargetPlaceProcess_Prefix()
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsServer) return true;

        // Let server perform the search target and LeaveDock()
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.ArriveBase))]
    public static bool ArriveBase_Prefix(DFRelayComponent __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRelayRequest;

        Multiplayer.Session.Network.SendPacket(new DFRelayArriveBasePacket(__instance));
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.ArriveDock))]
    public static bool ArriveDock_Prefix(DFRelayComponent __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRelayRequest;

        Multiplayer.Session.Network.SendPacket(new DFRelayArriveDockPacket(__instance));
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.LeaveBase))]
    public static bool LeaveBase_Prefix(DFRelayComponent __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRelayRequest;

        Multiplayer.Session.Network.SendPacket(new DFRelayLeaveBasePacket(__instance));
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.LeaveDock))]
    public static bool LeaveDock_Prefix(DFRelayComponent __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRelayRequest;

        Multiplayer.Session.Network.SendPacket(new DFRelayLeaveDockPacket(__instance));
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DFRelayComponent.LogicTickBaseMaintain))]
    public static bool LogicTickBaseMaintain_Prefix(DFRelayComponent __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        if (__instance.baseState == 2)
        {
            if (__instance.hive.galaxy.astrosFactory[__instance.targetAstroId] == null)
            {
                //the target factory is not loaded on client
                return false;
            }
        }
        return true;
    }
}
