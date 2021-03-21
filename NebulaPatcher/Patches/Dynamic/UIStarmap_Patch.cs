using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStarmap), "CheckOrCreateRTex")]
    class UIStarmap_Patch
    {
        static bool Prefix()
        {
            return !NebulaBootstrapper.isDedicated;
        }
    }
}
