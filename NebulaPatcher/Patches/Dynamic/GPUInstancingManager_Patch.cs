using HarmonyLib;
using NebulaPatcher.MonoBehaviours;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GPUInstancingManager), "GetObjectRenderer")]
    class GPUInstancingManager_Patch
    {
        static bool Prefix()
        {
            return !NebulaBootstrapper.isDedicated;
        }
    }
}
