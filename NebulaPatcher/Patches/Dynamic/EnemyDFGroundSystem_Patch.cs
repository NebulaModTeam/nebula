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
    [HarmonyPatch(nameof(EnemyDFGroundSystem.ExecuteDeferredEnemyChange))]
    public static bool ExecuteDeferredEnemyChange_Prefix(EnemyDFGroundSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (Multiplayer.Session.IsClient)
        {
            // Wait for server to authorize
            if (__instance._rmv_id_list != null && __instance._rmv_id_list.Count > 0)
            {
                __instance._rmv_id_list.Clear();
            }
            if (__instance._add_bidx_list != null && __instance._add_bidx_list.Count > 0)
            {
                __instance._add_bidx_list.Clear();
            }
            return false;
        }

        var planetId = __instance.planet.id;
        var starId = __instance.planet.star.id;
        if (__instance._rmv_id_list != null && __instance._rmv_id_list.Count > 0)
        {
            foreach (var enemyId in __instance._rmv_id_list)
            {
                __instance.factory.RemoveEnemyFinal(enemyId);
                var packet = new DeferredRemoveEnemyPacket(planetId, enemyId);
                Multiplayer.Session.Network.SendPacketToStar(packet, starId);
            }
            __instance._rmv_id_list.Clear();
        }
        if (__instance._add_bidx_list != null && __instance._add_bidx_list.Count > 0)
        {
            foreach (var (baseId, builderIndex) in __instance._add_bidx_list)
            {
                var enemyId = __instance.factory.CreateEnemyFinal(baseId, builderIndex);
                var packet = new DeferredCreateEnemyPacket(planetId, baseId, builderIndex, enemyId);
                Multiplayer.Session.Network.SendPacketToStar(packet, starId);
            }
            __instance._add_bidx_list.Clear();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.InitiateUnitDeferred))]
    public static bool InitiateUnitDeferred_Prefix()
    {
        // Do not call InitiateUnit in multiplayer game
        return !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.ExecuteDeferredUnitFormation))]
    public static bool ExecuteDeferredUnitFormation_Prefix(EnemyDFGroundSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (__instance._initiate_unit_list != null && __instance._initiate_unit_list.Count > 0)
        {
            __instance._initiate_unit_list.Clear();
        }

        if (Multiplayer.Session.IsClient)
        {
            // Wait for server to authorize
            if (__instance._deactivate_unit_list != null && __instance._deactivate_unit_list.Count > 0)
            {
                __instance._deactivate_unit_list.Clear();
            }
            return false;
        }

        if (__instance._deactivate_unit_list != null && __instance._deactivate_unit_list.Count > 0)
        {
            var planetId = __instance.planet.id;
            var starId = __instance.planet.star.id;
            foreach (var unitId in __instance._deactivate_unit_list)
            {
                var packet = new DeactivateGroundUnitPacket(planetId, unitId);
                Multiplayer.Session.Network.SendPacketToStar(packet, starId);
                __instance.DeactivateUnit(unitId);
            }
            __instance._deactivate_unit_list.Clear();
        }
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.NotifyEnemyKilled))]
    public static bool NotifyEnemyKilled_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        // Wait for server to authorize
        return Multiplayer.Session.Combat.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.PostKeyTick))]
    public static bool PostKeyTick_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        // Don't remove unit in formation because it may still waiting for server
        return false;
    }

}
