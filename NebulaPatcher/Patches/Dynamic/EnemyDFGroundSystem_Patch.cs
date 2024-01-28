#region

using HarmonyLib;
using NebulaModel.Packets.Combat.GroundEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyDFGroundSystem))]
internal class EnemyDFGroundSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.InitiateUnitDeferred))]
    public static bool InitiateUnitDeferred_Prefix()
    {
        // Do not call InitiateUnit in multiplayer game
        return !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.ExecuteDeferredUnitFormation))]
    public static void ExecuteDeferredUnitFormation_Prefix(EnemyDFGroundSystem __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsClient) return;

        if (__instance._deactivate_unit_list != null && __instance._deactivate_unit_list.Count > 0)
        {
            var planetId = __instance.planet.id;
            var starId = __instance.planet.star.id;
            foreach (var unitId in __instance._deactivate_unit_list)
            {
                var packet = new DeactivateGroundUnitPacket(planetId, unitId);
                Multiplayer.Session.Network.SendPacketToStar(packet, starId);
            }
        }
    }
}
