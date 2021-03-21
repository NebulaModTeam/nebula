using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarmap), "CheckOrCreateRTex")]
    class UIStarmap_Patch
    {
        static bool Prefix()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
    }
}
