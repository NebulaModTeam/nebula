using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Player;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MechaDroneLogic))]
    class MechaDroneLogic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("UpdateTargets")]
        public static void UpdateTargets_Prefix()
        {
            if (SimulatedWorld.Initialized)
            {
                DroneManager.ClearCachedPositions();
            }
        }
    }
}
