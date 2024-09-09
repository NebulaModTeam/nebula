#region

using HarmonyLib;
using NebulaWorld;
using UITools;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIControlPanelPlanetEntry))]
internal class UIControlPanelPlanetEntry_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIControlPanelPlanetEntry.UpdateBanner))]
    public static bool UpdateBanner_Prefix(UIControlPanelPlanetEntry __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        // Copy from vanilla code, except that ref to planet.factory is removed
        if (!__instance.isTargetDataValid)
        {
            return false;
        }
        if (__instance.isLocal)
        {
            __instance.distanceText.text = "当前星球".Translate();
            __instance.distanceText.font = __instance.masterWindow.FONT_SAIRASB;
        }
        else
        {
            var uPosition = __instance.planet.uPosition;
            var magnitude = (GameMain.mainPlayer.uPosition - uPosition).magnitude; // No need to access planet.factory in vanilla code
            var distanceLY = magnitude / 2400000.0;
            if (distanceLY < 0.10000000149011612)
            {
                var distanceAU = magnitude / 40000.0;
                __instance.distanceText.text = string.Format("距伊卡洛斯距离提示".Translate(), distanceAU.ToString("F1") + " AU");
            }
            else
            {
                __instance.distanceText.text = string.Format("距伊卡洛斯距离提示".Translate(), distanceLY.ToString("F1") + " ly");
            }
            __instance.distanceText.font = __instance.masterWindow.FONT_DIN;
        }
        __instance.planetTypeText.text = __instance.planet.typeString;
        var displayName = __instance.planet.displayName;
        Utils.UITextTruncateShow(__instance.planetNameText, displayName, __instance.planetNameTextWidthLimit, ref __instance.planetNameTextSettings);

        return false;
    }
}
