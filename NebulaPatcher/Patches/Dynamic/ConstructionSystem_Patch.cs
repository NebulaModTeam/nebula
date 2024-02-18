#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(ConstructionSystem))]
internal class ConstructionSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstructionSystem.UpdateDrones))]
    public static void UpdateDrones(ConstructionSystem __instance, ObjectRenderer[] renderers, bool sync_gpu_inst, float dt, long time)
    {
        if (!Multiplayer.IsActive) return;

        // Update remote drones from other players
        var factory = __instance.factory;
        Multiplayer.Session.Drones.UpdateDrones(factory, renderers, sync_gpu_inst, dt, time);
    }
}
