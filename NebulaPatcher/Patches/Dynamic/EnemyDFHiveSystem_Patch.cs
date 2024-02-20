#region

using HarmonyLib;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyDFHiveSystem))]
internal class EnemyDFHiveSystem_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ExecuteDeferredEnemyChange))]
    public static bool ExecuteDeferredEnemyChange_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient)
        {
            __instance._add_relay_list?.Clear();
            __instance._add_tinder_list?.Clear();
            __instance._rmv_id_list?.Clear();
            __instance._add_bidx_list?.Clear();
            return false;
        }

        var hiveAstroId = __instance.hiveAstroId;
        if (__instance._add_relay_list != null && __instance._add_relay_list.Count > 0)
        {
            foreach (var dockIndex in __instance._add_relay_list)
            {
                ref var ptr = ref __instance.relayDocks[dockIndex % __instance.relayDocks.Length];
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, 8116, __instance.hiveAstroId, ptr.pos, ptr.rot);
                var dfRelayId = __instance.sector.enemyPool[enemyId].dfRelayId;
                var dfrelayComponent = __instance.relays.buffer[dfRelayId];
                if (dfrelayComponent != null)
                {
                    dfrelayComponent.SetDockIndex(dockIndex);
                    __instance.AddIdleRelay(dfRelayId);
                }
                Multiplayer.Session.Network.SendPacket(new DFSAddIdleRelayPacket(hiveAstroId, dockIndex, enemyId));
            }
            __instance._add_relay_list.Clear();
        }
        if (__instance._add_tinder_list != null && __instance._add_tinder_list.Count > 0)
        {
            foreach (var dockIndex in __instance._add_tinder_list)
            {
                ref var ptr2 = ref __instance.tinderDocks[dockIndex % __instance.tinderDocks.Length];
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, 8119, __instance.hiveAstroId, ptr2.pos, ptr2.rot);
                var dfTinderId = __instance.sector.enemyPool[enemyId].dfTinderId;
                ref var tinder = ref __instance.tinders.buffer[dfTinderId];
                if (dfTinderId > 0)
                {
                    tinder.SetDockIndex(__instance, dockIndex);
                    __instance.AddIdleTinder(dfTinderId);
                }
                Multiplayer.Session.Network.SendPacket(new DFSAddIdleTinderPacket(hiveAstroId, dockIndex, enemyId));
            }
            __instance._add_tinder_list.Clear();
            __instance._add_tinder_list = null;
        }
        if (!__instance.realized)
        {
            return false;
        }
        if (__instance._rmv_id_list != null && __instance._rmv_id_list.Count > 0)
        {
            foreach (var enemyId in __instance._rmv_id_list)
            {
                __instance.sector.RemoveEnemyFinal(enemyId);
                Multiplayer.Session.Network.SendPacket(new DFSRemoveEnemyDeferredPacket(enemyId));
            }
            __instance._rmv_id_list.Clear();
        }
        if (__instance._add_bidx_list != null && __instance._add_bidx_list.Count > 0)
        {
            foreach (var builderIndex in __instance._add_bidx_list)
            {
                var enemyId = __instance.sector.CreateEnemyFinal(__instance, builderIndex, false);
                Multiplayer.Session.Network.SendPacket(new DFSAddEnemyDeferredPacket(hiveAstroId, builderIndex, enemyId));
            }
            __instance._add_bidx_list.Clear();
        }
        return false;
    }

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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.Realize))]
    public static bool Realize_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        if (!__instance.realized)
        {
            Multiplayer.Session.Network.SendPacket(new DFHiveRealizePacket(__instance.hiveAstroId));
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.OpenPreview))]
    public static bool OpenPreview_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (!Multiplayer.Session.Enemies.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DFHivePreviewPacket(__instance.hiveAstroId, true));
        }
        return Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.ClosePreview))]
    public static bool ClosePreview_Prefix(EnemyDFHiveSystem __instance)
    {
        if (!Multiplayer.IsActive) return true;

        if (!Multiplayer.Session.Enemies.IsIncomingRequest)
        {
            Multiplayer.Session.Network.SendPacket(new DFHivePreviewPacket(__instance.hiveAstroId, false));
        }
        return Multiplayer.Session.IsServer || Multiplayer.Session.Enemies.IsIncomingRequest.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyDFHiveSystem.KillCorruptedUnits))]
    public static bool KillCorruptedUnits_Prefix()
    {
        // Don't run enemy position check on client
        return !Multiplayer.IsActive || Multiplayer.Session.IsServer;
    }
}
