#region

using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.SpaceEnemy;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(SpaceSector))]
internal class SpaceSector_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.SetForNewGame))]
    public static void SetForNewGame_Prefix(SpaceSector __instance)
    {
        if (Multiplayer.IsActive && Multiplayer.Session.IsClient)
        {
            // Set this.isCombatMode to false to skip the part of initial enemyDFHiveSystem
            // Will revert it to the correct value during SpaceSector.Import called in GameStatesManager.OverwriteGlobalGameData
            __instance.isCombatMode = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.KillEnemyFinal))]
    public static bool KillEnemyFinal_Prefix(SpaceSector __instance, int enemyId)
    {
        if (!Multiplayer.IsActive || enemyId <= 0)
        {
            return true;
        }
        ref var enemyPtr = ref __instance.enemyPool[enemyId];
        if (Multiplayer.Session.IsServer)
        {
            Multiplayer.Session.Network.SendPacket(new DFSKillEnemyPacket(enemyPtr.originAstroId, enemyId));
            return true;
        }
        if (Multiplayer.Session.Enemies.IsIncomingRequest.Value)
        {
            return true;
        }

        // Client: wait for server to approve the unitId and enmeyId recycle
        // Make this enemyData appear as empty        
        enemyPtr.isInvincible = true;
        enemyPtr.id = 0;
        Multiplayer.Session.Network.SendPacket(new DFSKillEnemyPacket(enemyPtr.originAstroId, enemyId));

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.RemoveEnemyWithComponents))]
    public static void RemoveEnemyWithComponents_Prefix(SpaceSector __instance, int id)
    {
        // Fix IndexOutOfRangeException in SpaceSector.RemoveEnemyWithComponents IL_026A 
        // This is due to combatStats is not sync in client
        if (id != 0 && __instance.enemyPool[id].id != 0)
        {
            if (__instance.enemyPool[id].combatStatId != 0)
            {
                if (__instance.enemyPool[id].combatStatId >= __instance.skillSystem.combatStats.cursor)
                    __instance.enemyPool[id].combatStatId = 0;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.TryCreateNewHive))]
    public static bool TryCreateNewHive_Prefix(StarData star)
    {
        if (!Multiplayer.IsActive) return true;
        if (Multiplayer.Session.IsClient) return Multiplayer.Session.Enemies.IsIncomingRequest;

        if (star != null)
        {
            Multiplayer.Session.Network.SendPacket(new DFHiveCreateNewHivePacket(star.id));
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SpaceSector.GameTick))]
    public static void GameTick_Prefix(SpaceSector __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return;

        // Fix NRE in DFSTurretComponent.InternalUpdate (PrefabDesc pdesc);(IL_0017)
        for (var enemyId = 1; enemyId < __instance.enemyCursor; enemyId++)
        {
            ref var enemy = ref __instance.enemyPool[enemyId];
            if (enemy.id != enemyId) continue;

            if (SpaceSector.PrefabDescByModelIndex[enemy.modelIndex] == null)
            {
                var msg = $"Remove SpaceSector enemy[{enemyId}]: modelIndex{enemy.modelIndex}";
                Log.WarnInform(msg);

                __instance.enemyPool[enemyId].SetEmpty();
            }
        }
    }
}
