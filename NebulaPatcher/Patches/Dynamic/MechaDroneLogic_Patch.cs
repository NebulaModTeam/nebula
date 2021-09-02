using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(MechaDroneLogic))]
    class MechaDroneLogic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(MechaDroneLogic.UpdateTargets))]
        public static void UpdateTargets_Prefix()
        {
            if (Multiplayer.IsActive)
            {
                Multiplayer.Session.Drones.ClearCachedPositions();
            }
        }
    }
}
