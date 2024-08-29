#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelWindow))]
internal class UIControlPanelWindow_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.Last)]
    [HarmonyPatch(nameof(UIControlPanelWindow.DetermineFilterResults))]
    public static bool DetermineFilterResults_Prefix(UIControlPanelWindow __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        // Send request to server and wait for response
        __instance.needDetermineFilterResults = false;
        Multiplayer.Session.Client.SendPacket(new LCPFilterResultsRequest(__instance.filter));
        return false;
    }
}
