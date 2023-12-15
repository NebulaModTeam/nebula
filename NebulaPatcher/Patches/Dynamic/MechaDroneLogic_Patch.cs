#region

using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Player;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

//todo:replace
//[HarmonyPatch(typeof(MechaDroneLogic))]
internal class MechaDroneLogic_Patch
{
    //[HarmonyPrefix]
    //[HarmonyPatch(nameof(MechaDroneLogic.UpdateTargets))]
    public static void UpdateTargets_Prefix()
    {
        if (Multiplayer.IsActive)
        {
            DroneManager.ClearCachedPositions();
        }
    }
}
