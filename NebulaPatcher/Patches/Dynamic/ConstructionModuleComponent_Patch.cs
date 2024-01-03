using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(ConstructionModuleComponent))]
internal class ConstructionModuleComponent_Patch
{
    // dont give back idle construction drones to player if it was a drone owned by a remote player
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ConstructionModuleComponent.RecycleDrone))]
    public static void RecycleDrone_Postfix(ConstructionModuleComponent __instance, ref DroneComponent drone)
    {
        if (!Multiplayer.IsActive)
        {
            return;
        }

        if (drone.owner < 0 && drone.owner * -1 != Multiplayer.Session.LocalPlayer.Id)
        {
            __instance.droneIdleCount--;
        }
    }

    // clients should skip the procedure for BattleBases. The host will tell them when to eject drones.
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstructionModuleComponent.IdleDroneProcedure))]
    public static bool IdleDroneProcedure_Prefix(ConstructionModuleComponent __instance)
    {
        if (!Multiplayer.IsActive)
        {
            return true;
        }

        return !(Multiplayer.Session.LocalPlayer.IsClient && __instance.entityId > 0);
    }
}
