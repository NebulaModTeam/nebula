using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(TranslucentImageSource), "CreateNewBlurredScreen")]
    class TranslucentImageSource_Patch
    {
        static bool Prefix()
        {
            return !NebulaBootstrapper.isDedicated;
        }
    }
}
