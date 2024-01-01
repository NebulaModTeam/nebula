#region

using HarmonyLib;
using NebulaModel;
using NebulaModel.Logger;
using NebulaNetwork;
using NebulaWorld;
using static NebulaPatcher.Patches.Dynamic.UIOptionWindow_Patch;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UILoadGameWindow))]
internal class UILoadGameWindow_Patch
{
    private static Tooltip loadDisabledTooltip;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UILoadGameWindow.DoLoadSelectedGame))]
    public static void DoLoadSelectedGame_Postfix()
    {
        if (!Multiplayer.IsInMultiplayerMenu)
        {
            return;
        }
        Log.Info($"Listening server on port {Config.Options.HostPort}");
        Multiplayer.HostGame(new Server(Config.Options.HostPort, true));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UILoadGameWindow.OnSelectedChange))]
    public static void OnSelectedChange_Postfix(UILoadGameWindow __instance)
    {
        if (!Multiplayer.IsInMultiplayerMenu)
        {
            RemoveLoadDisabledTooltip();
            return;
        }

#if RELEASE
        DisableLoadIfSaveHasCombatModeEnabled(__instance);
#endif
    }

    private static void DisableLoadIfSaveHasCombatModeEnabled(UILoadGameWindow uiLoadGameWindow)
    {
        if (uiLoadGameWindow.selected?.saveName != null && (GameSave.LoadGameDesc(uiLoadGameWindow.selected.saveName)?.isCombatMode ?? false))
        {
            uiLoadGameWindow.loadButton.button.interactable = false;

            if (loadDisabledTooltip == null)
            {
                loadDisabledTooltip = uiLoadGameWindow.loadButton.button.gameObject.AddComponent<Tooltip>();
                loadDisabledTooltip.Title = "Not supported in multiplayer";
                loadDisabledTooltip.Text = "Loading saved games with combat mode enabled is currently not supported in multiplayer.";
            }
        }
        else
        {
            RemoveLoadDisabledTooltip();
        }
    }

    private static void RemoveLoadDisabledTooltip()
    {
        if (loadDisabledTooltip != null)
        {
            Object.Destroy(loadDisabledTooltip);
        }
    }
}
