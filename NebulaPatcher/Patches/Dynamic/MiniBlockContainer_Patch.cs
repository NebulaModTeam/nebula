using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MiniBlockContainer), "Draw")]
    class MiniBlockContainer_Patch
    {
        static bool Prefix()
        {
            return !MultiplayerHostSession.isDedicated;
        }
    }
}
