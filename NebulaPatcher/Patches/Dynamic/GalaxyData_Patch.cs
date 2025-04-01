#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(GalaxyData))]
internal class GalaxyData_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GalaxyData.UpdateScanningProcedure))]
    public static bool UpdateScanningProcedure_Prefix()
    {
        // The scanning in multiplayer should be triggered by user interactions, instead of vanilla behavior (auto call for every 10s)
        return !Multiplayer.IsActive;
    }
}
