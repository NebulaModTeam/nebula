#region

using HarmonyLib;
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
}
