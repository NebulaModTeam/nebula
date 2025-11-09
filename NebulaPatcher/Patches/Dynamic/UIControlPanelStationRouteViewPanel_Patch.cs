#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelStationRouteViewPanel))]
internal class UIControlPanelStationRouteViewPanel_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelStationRouteViewPanel.DetermineRouteResult))]
    public static bool DetermineRouteResult_Prefix(UIControlPanelStationRouteViewPanel __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer)
        {
            return true;
        }

        // For client's PLS, use local result
        if (__instance.inspectorFactory == null
            || __instance.inspectorStation == null
            || __instance.inspectorStation.gid == 0)
        {
            return true;
        }

        // Query the result from the server
        var packet = new LCPStationRouteResultsPacket
        {
            QueryPlanetId = __instance.inspectorFactory.planetId,
            QueryStationGid = __instance.inspectorStation.gid
        };
        Multiplayer.Session.Client.SendPacket(packet);

        // Wait for the response to update
        __instance.needDetermineRouteResult = false;
        return false;
    }
}
