using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSwarm), "Init")]
    class DysonSwarm_Patch
    {
        static bool Prefix()
        {
            //This methods calculates shaders on DysonSwarm which causes crashes in dedicated mode.
            return !NebulaBootstrapper.isDedicated;
        }
    }
} 
