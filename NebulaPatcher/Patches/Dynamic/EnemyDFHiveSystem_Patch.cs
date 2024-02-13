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
    [HarmonyPatch(nameof(EnemyDFHiveSystem.NotifyRelayKilled))]
    public static bool NotifyRelayKilled(EnemyDFHiveSystem __instance, ref EnemyData enemy)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        var dfrelayComponent = __instance.relays.buffer[enemy.dfRelayId];
        if (dfrelayComponent != null && dfrelayComponent.id == enemy.dfRelayId)
        {
            __instance.relayNeutralizedCounter++;
            if (dfrelayComponent.baseState == 1 && dfrelayComponent.stage == 2)
            {
                var planetData = __instance.sector.galaxy.PlanetById(dfrelayComponent.targetAstroId);
                if (planetData != null)
                {
                    //Don't call GetOrCreateFactory in client. Only realize if the factory is already load from server
                    if (planetData.factory != null)
                    {
                        dfrelayComponent.RealizePlanetBase(__instance.sector);
                    }
                }
            }
        }
        return false;
    }
}
