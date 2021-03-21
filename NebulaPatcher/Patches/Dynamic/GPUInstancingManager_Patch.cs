using HarmonyLib;
using NebulaHost;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GPUInstancingManager), "GetObjectRenderer")]
    class GPUInstancingManager_Patch
    {
        static bool Prefix()
        {
            return !MultiplayerHostSession.IsDedicated;
        }
    }
}
