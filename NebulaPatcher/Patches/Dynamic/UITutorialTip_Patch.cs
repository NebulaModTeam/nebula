#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UITutorialTip))]
public class UITutorialTip_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UITutorialTip.PopupTutorialTip))]
    public static bool PopupTutorialTip_Prefix(int tutorialId)
    {
        if (!Multiplayer.IsActive) return true;

        // In MP, disable tutorial tip so they don't show up when login every time
        GameMain.history.UnlockTutorial(tutorialId);
        return false;
    }
}
