#region

using HarmonyLib;
using NebulaModel.Packets.Logistics.ControlPanel;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelWindow))]
internal class UIControlPanelWindow_Patch
{
    public static int UpdateTimer { get; private set; }

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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelWindow.DetermineSelectionInspector))]
    public static bool DetermineSelectionInspectorPrefix(UIControlPanelWindow __instance)
    {
        if (!Multiplayer.IsActive) return true;

        // In MP, open the local station window instead of inspector temporarily
        // TODO: Enable Inspector in client for remote entry and sync
        __instance.needDetermineSelectionInspector = false;
        var planet = GameMain.galaxy.PlanetById(__instance.selection.astroId);
        var factory = planet?.factory;
        if (factory == null || GameMain.localPlanet != planet) return false;
        switch (__instance.selection.entryType)
        {
            case EControlPanelEntryType.InterstellarStation:
            case EControlPanelEntryType.OrbitCollector:
            case EControlPanelEntryType.LocalStation:
            case EControlPanelEntryType.VeinCollector:
                // Close station window first so it can stay on top
                UIRoot.instance.uiGame.ShutStationWindow();
                var minerId = factory.entityPool[__instance.selection.objId].minerId;
                var stationId = factory.entityPool[__instance.selection.objId].stationId;
                UIRoot.instance.uiGame.stationWindow.veinCollectorPanel.minerId = minerId;
                UIRoot.instance.uiGame.stationWindow.stationId = stationId;
                if (UIRoot.instance.uiGame.inspectStationId == 0 && stationId > 0)
                {
                    UIRoot.instance.uiGame.OpenStationWindow();
                }
                break;

            case EControlPanelEntryType.Dispenser:
                // Close station window first so it can stay on top
                UIRoot.instance.uiGame.ShutDispenserWindow();
                var dispenserId = factory.entityPool[__instance.selection.objId].dispenserId;
                UIRoot.instance.uiGame.dispenserWindow.dispenserId = dispenserId;
                if (UIRoot.instance.uiGame.inspectDispenserId == 0 && dispenserId > 0)
                {
                    UIRoot.instance.uiGame.OpenDispenserWindow();
                }
                break;
        }
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelWindow._OnUpdate))]
    public static void OnUpdate_Postfix()
    {
        UpdateTimer = (++UpdateTimer) % 600;
    }
}
