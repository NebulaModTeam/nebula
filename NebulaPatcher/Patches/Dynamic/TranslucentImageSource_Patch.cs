using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(TranslucentImageSource), "CreateNewBlurredScreen")]
    class TranslucentImageSource_Patch
    {
        static bool Prefix()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
    }
}
