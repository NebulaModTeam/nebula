#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStationRoutePanel))]
internal class UIStationRoutePanel_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationRoutePanel.OnSearchNameInputChanged))]
    public static bool OnSearchNameInputChanged_Prefix(UIStationRoutePanel __instance, string str)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || string.IsNullOrEmpty(str))
        {
            return true;
        }
        // Query and wait for the result from server
        var packet = new LCPStationNameSearchPacket
        {
            SearchString = str,
            IsExact = false,
            LocalPlanetId = __instance.station.planetId
        };
        Multiplayer.Session.Client.SendPacket(packet);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationRoutePanel.OnSearchNameInputEndEdit))]
    public static bool OnSearchNameInputEndEdit_Prefix(UIStationRoutePanel __instance, string str)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer || string.IsNullOrEmpty(str))
        {
            return true;
        }
        // Query and wait for the result from server
        var packet = new LCPStationNameSearchPacket
        {
            SearchString = str,
            IsExact = true,
            LocalPlanetId = __instance.station.planetId
        };
        Multiplayer.Session.Client.SendPacket(packet);
        __instance.activeSearchEntryCount = 0;
        __instance.searchDropDownRt.gameObject.SetActive(false);
        return false;
    }
}
