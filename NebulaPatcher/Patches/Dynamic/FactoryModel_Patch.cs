#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(FactoryModel))]
internal class FactoryModel_Patch
{
    // if a custom birth planet is set we need to unload planet factory data as GameMain.data is destroyed while loading into the game
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FactoryModel.SetGlobalRenderState))]
    public static bool SetGlobalRenderState_Prefix(FactoryModel __instance)
    {
        if (GameMain.mainPlayer != null || GameMain.data != null)
        {
            return true;
        }
        // GameMain.data is destroyed while loading into the game, but we had the planets factory loaded in galaxy select when selecting a custom birth planet.
        // thus unload the data here to prevent nre spam.
        __instance.planet.UnloadFactory();
        return false;
    }
}
