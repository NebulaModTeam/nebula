using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MiniBlockContainer), "Draw")]
    class MiniBlockContainer_Patch
    {
        static bool Prefix()
        {
            return !NebulaBootstrapper.isDedicated;
        }
    }
}
