#region

using System;
using HarmonyLib;
using NebulaModel.Logger;
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
            __instance._rmv_id_list?.Clear();
            __instance._add_bidx_list?.Clear();
            return false;
        }

        var planetId = __instance.planet.id;
        var starId = __instance.planet.star.id;
        if (__instance._rmv_id_list?.Count > 0)
        {
            foreach (var enemyId in __instance._rmv_id_list)
            {
                __instance.factory.RemoveEnemyFinal(enemyId);
                var packet = new DFGDeferredRemoveEnemyPacket(planetId, enemyId);
                Multiplayer.Session.Network.SendPacketToStar(packet, starId);
            }
            __instance._rmv_id_list.Clear();
        }
        if (__instance._add_bidx_list?.Count > 0)
        {
            foreach (var (baseId, builderIndex) in __instance._add_bidx_list)
            {
                var enemyId = __instance.factory.CreateEnemyFinal(baseId, builderIndex);
                var packet = new DFGDeferredCreateEnemyPacket(planetId, baseId, builderIndex, enemyId);
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
        // Skip InitiateUnit in multiplayer game
        return !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.ExecuteDeferredUnitFormation))]
    public static bool ExecuteDeferredUnitFormation_Prefix(EnemyDFGroundSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient)
        {
            // Wait for server to authorize
            __instance._initiate_unit_list?.Clear();
            __instance._deactivate_unit_list?.Clear();
            return false;
        }

        // Skip InitiateUnit in multiplayer game
        __instance._initiate_unit_list?.Clear();
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.CanEraseBase))]
    public static bool CanEraseBase_Prefix(DFGBaseComponent _base, ref bool __result)
    {
        if (!Multiplayer.IsActive) return true;

        if (_base == null || _base.id == 0)
        {
            __result = true;
            return false;
        }
        // Skip __instance.builders.buffer[_base.builderId].sp check as it may have different value
        var pbuilders = _base.pbuilders;
        for (var i = 2; i < pbuilders.Length; i++)
        {
            if (pbuilders[i].instId > 0)
            {
                __result = false;
                return false;
            }
        }
        __result = true;
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
    [HarmonyPatch(nameof(EnemyDFGroundSystem.NotifyBaseRemoving))]
    public static bool NotifyBaseRemoving_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        // Wait for DFRelayLeaveBasePacket from server
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.PostKeyTick))]
    public static bool PostKeyTick_Prefix()
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;
        // Don't remove unit in formation because it may still waiting for server
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.RemoveBasePit))]
    public static bool RemoveBasePit_Prefix(EnemyDFGroundSystem __instance, int pitRuinId)
    {
        if (!Multiplayer.IsActive) return true;

        var buffer = __instance.bases.buffer;
        var cursor = __instance.bases.cursor;
        for (var baseId = 1; baseId < cursor; baseId++)
        {
            var dfgbaseComponent = buffer[baseId];
            if (dfgbaseComponent?.id == baseId && dfgbaseComponent.ruinId == pitRuinId)
            {
                var packet = new DFGRemoveBasePitPacket(__instance.factory.planetId, baseId);
                if (Multiplayer.Session.IsServer)
                {
                    Multiplayer.Session.Network.SendPacketToStar(packet, __instance.factory.planet.star.id);
                    return true;
                }
                else
                {
                    // Request server to remove base pit
                    Multiplayer.Session.Network.SendPacket(packet);
                    return false;
                }
            }
        }
        // Is it possible to go here in vanilla?
        __instance.factory.RemoveRuinWithComponet(pitRuinId);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.RemoveDFGBaseComponent))]
    public static bool RemoveDFGBaseComponent_Prefix(EnemyDFGroundSystem __instance, int id)
    {
        if (!Multiplayer.IsActive) return true;

        if (Multiplayer.Session.IsServer)
        {
            var packet = new DFGRemoveBasePacket(__instance.factory.planetId, id);
            Multiplayer.Session.Network.SendPacketToStar(packet, __instance.factory.planet.star.id);
            return true;
        }

        // Client should wait for server to approve the removal of base from the base buffer
        return Multiplayer.Session.Combat.IsIncomingRequest;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.GameTickLogic))]
    public static void GameTickLogic_Prefix(EnemyDFGroundSystem __instance, long gameTick)
    {
        if (!Multiplayer.IsActive) return;

        var planetId = __instance.planet.id;
        var targets = Multiplayer.Session.Enemies.GroundTargets;
        if (!targets.TryGetValue(planetId, out var array) || array.Length < __instance.factory.enemyCapacity)
        {
            targets[planetId] = new int[__instance.factory.enemyCapacity];
            if (array != null)
            {
                Array.Copy(array, targets[planetId], array.Length);
            }
        }

        if (Multiplayer.Session.IsServer)
        {
            // Broadcast base level changes before adding units
            Multiplayer.Session.Enemies.BroadcastBaseStatusPackets(__instance, gameTick);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFGroundSystem.KeyTickLogic))]
    public static void KeyTickLogic_Prefix(EnemyDFGroundSystem __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return;

        // Fix NRE in EnemyDFGroundSystem.KeyTickLogic (System.Int64 time);(IL_0929)
        var cursor = __instance.builders.cursor;
        var buffer = __instance.builders.buffer;
        var baseBuffer = __instance.bases.buffer;
        var enemyPool = __instance.factory.enemyPool;
        for (var builderId = 1; builderId < cursor; builderId++)
        {
            ref var builder = ref buffer[builderId];
            if (builder.id == builderId)
            {
                if (baseBuffer[enemyPool[builder.enemyId].owner] == null)
                {
                    var msg = $"Remove EnemyDFGroundSystem enemy[{builder.enemyId}]: owner = {enemyPool[builder.enemyId].owner}";
                    Log.WarnInform(msg);

                    __instance.factory.enemyPool[builder.enemyId].SetEmpty();
                    __instance.builders.Remove(builderId);
                }
            }
        }
    }

}
