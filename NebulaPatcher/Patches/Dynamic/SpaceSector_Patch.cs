#region

using HarmonyLib;
using NebulaWorld;
using NebulaModel.Packets.Combat.DFHive;
using NebulaModel.Packets.Combat.SpaceEnemy;

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
}
