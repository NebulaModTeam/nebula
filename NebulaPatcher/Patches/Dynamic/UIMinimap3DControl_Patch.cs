using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMinimap3DControl), "_OnCreate")]
    class UIMinimap3DControl_Patch
    {
        static bool Prefix()
        {
            return !MultiplayerHostSession.isDedicated;
        }
    }
}
