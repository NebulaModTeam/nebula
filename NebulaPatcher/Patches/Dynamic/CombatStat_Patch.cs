#region

using HarmonyLib;
using NebulaModel.Packets.Combat;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(CombatStat))]
internal class CombatStat_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CombatStat.TickSkillLogic))]
    public static void TickSkillLogic_Prefix(ref CombatStat __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return;

        // objectType 0:entity
        if (__instance.objectType == 0)
        {
            // Client: leave building hp at 1 until server send Kill event
            var newHp = __instance.hp + __instance.hpRecover;
            if (newHp <= 0)
            {
                __instance.hp = 1;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CombatStat.HandleFullHp))]
    public static void HandleFullHp_Prefix(ref CombatStat __instance)
    {
        if (!Multiplayer.IsActive) return;

        // objectType 0:entity
        if (__instance.objectType == 0 && __instance.originAstroId > 100 && __instance.originAstroId <= 204899 && __instance.originAstroId % 100 > 0)
        {
            var packet = new CombatStatFullHpPacket(__instance.originAstroId, __instance.objectType, __instance.objectId);
            if (Multiplayer.Session.IsServer)
            {
                var starId = __instance.originAstroId / 100;
                Multiplayer.Session.Server.SendPacketToStar(packet, starId);
            }
            else
            {
                Multiplayer.Session.Client.SendPacket(packet);
            }
        }
    }
}
