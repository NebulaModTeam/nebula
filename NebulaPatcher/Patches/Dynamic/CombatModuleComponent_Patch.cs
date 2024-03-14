#region

using System;
using HarmonyLib;
using NebulaModel.Logger;

#endregion

namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(CombatModuleComponent))]
internal class CombatModuleComponent_Patch
{
    static int s_errorCount = 0;

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(CombatModuleComponent), nameof(CombatModuleComponent.GameTick))]
    public static Exception GameTick_Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            // After 10 exception triggered, suppress the following messages
            if (s_errorCount++ < 10)
            {
                var msg = "GameTick_Finalizer suppressed exception: \n" + __exception.ToString();
                Log.Error(msg);
            }
        }
        return null;
    }
}
