#region

using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EnemyFormation))]
internal class EnemyFormation_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EnemyFormation), nameof(EnemyFormation.RemoveUnit))]
    public static bool RemoveUnit_Prefix(EnemyFormation __instance, int port)
    {
        if (__instance.units[port] != 0)
        {
            if (__instance.vacancyCursor < __instance.vacancies.Length) // guard
            {
                __instance.vacancies[__instance.vacancyCursor++] = port;
            }
            __instance.units[port] = 0;
        }
        return false;
    }
}
