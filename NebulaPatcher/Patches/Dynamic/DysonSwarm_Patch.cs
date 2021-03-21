using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(DysonSwarm), "Init")]
    class DysonSwarm_Patch
    {
        static bool Prefix()
        {
            //This methods calculates shaders on DysonSwarm which causes crashes in dedicated mode.
            return !MultiplayerHostSession.isDedicated;
        }
    }
} 
