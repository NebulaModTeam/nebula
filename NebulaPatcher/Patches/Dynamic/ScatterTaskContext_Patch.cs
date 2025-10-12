#region

using System;
using HarmonyLib;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(ScatterTaskContext))]
internal class ScatterTaskContext_Patch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ScatterTaskContext.ResetFrame), new Type[] { typeof(int), typeof(int) })]
    [HarmonyPatch(nameof(ScatterTaskContext.ResetFrame), new Type[] { typeof(long), typeof(int), typeof(int) })]
    public static void ResetFrame_Prefix(ref int _batchCount)
    {
        // Avoid DivideByZeroException
        if (_batchCount == 0) _batchCount = 1;
    }
}
