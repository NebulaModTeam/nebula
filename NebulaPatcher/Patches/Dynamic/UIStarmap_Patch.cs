using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarmap))]
    class UIStarmap_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStarmap._OnLateUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnLateUpdate_Postfix(UIStarmap __instance)
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.World.RenderPlayerNameTagsOnStarmap(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStarmap._OnClose))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnClose_Postfix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.World.ClearPlayerNameTagsOnStarmap();
            }
        }
    }
}
