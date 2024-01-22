#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(CombatStat))]
internal class CombatStat_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CombatStat.TickSkillLogic))]
    public static void TickSkillLogic_Prefix(ref CombatStat __instance)
    {
        if (NebulaWorld.Combat.CombatManager.LockBuildHp)
        {
            if (__instance.objectType == 0)
            {
                __instance.hp = __instance.hpMax;
            }
        }
    }
}
