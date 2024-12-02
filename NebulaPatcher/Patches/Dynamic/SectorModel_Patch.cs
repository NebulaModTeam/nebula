#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SectorModel))]
internal class SectorModel_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SectorModel.OnCameraPostRender))]
    public static bool OnCameraPostRender_Prefix()
    {
        if (!GameMain.isPaused && !GameMain.inOtherScene)
        {
            // Skip if GameMain.mainPlayer is null to prevent NRE when player joining the game
            return GameMain.mainPlayer != null;
        }
        return true;
    }
}
