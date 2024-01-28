#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyDFGroundSystem))]
internal class EnemyDFGroundSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.InitiateUnitDeferred))]
    public static bool InitiateUnitDeferred()
    {
        // Do not call InitiateUnit in multiplayer game
        return !Multiplayer.IsActive;
    }
}
