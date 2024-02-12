#region

using HarmonyLib;
using NebulaWorld;
#pragma warning disable IDE1006 // Naming Styles

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(EvolveData))]
internal class EvolveData_Patch
{
    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(EvolveData.AddExp))]
    public static bool AddExp_Prefix(ref EvolveData __instance, ref int _addexp)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        //Only increase exp point in client. Wait for server to level up
        if (_addexp > 0)
        {
            __instance.expf += _addexp;
            if (__instance.expf >= EvolveData.levelExps[__instance.level])
            {
                __instance.expf = EvolveData.levelExps[__instance.level] - 1;
            }
            _addexp = 0;
        }
        return false;
    }


    [HarmonyPrefix, HarmonyPriority(Priority.High)]
    [HarmonyPatch(nameof(EvolveData.AddExpPoint))]
    public static bool AddExpPoint_Prefix(ref EvolveData __instance, ref int _addexpp)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.IsServer) return true;

        //Only increase exp point in client. Wait for server to level up
        if (_addexpp > 0)
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
