using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIMinimap3DControl), "_OnCreate")]
    class UIMinimap3DControl_Patch
    {
        static bool Prefix()
        {
            return !NebulaBootstrapper.isDedicated;
        }
    }
}
