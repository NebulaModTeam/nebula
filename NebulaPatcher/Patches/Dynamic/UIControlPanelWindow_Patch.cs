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

        __instance.needDetermineSelectionInspector = false;
        var planet = GameMain.galaxy.PlanetById(__instance.selection.astroId);
        var factory = planet?.factory;
        switch (__instance.selection.entryType)
        {
            case EControlPanelEntryType.InterstellarStation:
            case EControlPanelEntryType.OrbitCollector:
            case EControlPanelEntryType.LocalStation:
            case EControlPanelEntryType.VeinCollector:
                // Open station inspector only if the factory is loaded
                if (factory == null)
                {
                    return false;
                }
                else return true;

            case EControlPanelEntryType.Dispenser:
                // In MP, temporarily disable the dispenser inspector and use the original dispenser window
                if (factory == null || GameMain.localPlanet != planet)
                {
                    return false;
                }
                // Close dispenser window first so it can stay on top
                UIRoot.instance.uiGame.ShutDispenserWindow();
                var dispenserId = factory.entityPool[__instance.selection.objId].dispenserId;
                UIRoot.instance.uiGame.dispenserWindow.dispenserId = dispenserId;
                if (UIRoot.instance.uiGame.inspectDispenserId == 0 && dispenserId > 0)
                {
                    UIRoot.instance.uiGame.OpenDispenserWindow();
                }
                return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIControlPanelWindow._OnUpdate))]
    public static void OnUpdate_Postfix()
    {
        UpdateTimer = (++UpdateTimer) % 600;
    }
}
