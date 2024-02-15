#region

using HarmonyLib;
using NebulaModel.Packets.Combat.DFHive;
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
}
