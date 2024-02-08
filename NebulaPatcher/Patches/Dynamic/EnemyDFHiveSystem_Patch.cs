#region

using HarmonyLib;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyDFHiveSystem))]
internal class EnemyDFHiveSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.KeyTickLogic))]
    public static bool KeyTickLogic_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        // Skip KeyTickLogic in client
        return false;
    }
}
