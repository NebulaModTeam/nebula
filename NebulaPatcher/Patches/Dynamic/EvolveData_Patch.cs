#region

using HarmonyLib;
using NebulaWorld;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EvolveData))]
internal class EvolveData_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(EvolveData.AddExpPoint))]
    public static bool AddExpPoint_Prefix(ref EvolveData __instance, ref int _addexpp)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        //Only increase exp point in client. Wait for server to level up
        if (_addexpp > 0 && __instance.level < 30)
        {
            __instance.expp += _addexpp;
            if (__instance.expp >= 10000)
            {
                __instance.expf += __instance.expp / 10000;
                __instance.expp %= 10000;
                if (__instance.expf >= EvolveData.levelExps[__instance.level])
                {
                    __instance.expf = EvolveData.levelExps[__instance.level] - 1;
                }
            }
            _addexpp = 0;
        }
        return false;
    }
}
