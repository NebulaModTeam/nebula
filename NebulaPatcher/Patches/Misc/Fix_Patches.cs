#region

using System;
using HarmonyLib;
using UnityEngine;

#endregion

namespace NebulaPatcher.Patches.Misc;

// Collections of patches to deal with bugs that root cause is unknown
internal class Fix_Patches
{
    // IndexOutOfRangeException: Index was outside the bounds of the array.
    // at BuildTool.GetPrefabDesc (System.Int32 objId)[0x0000e] ; IL_000E
    // at BuildTool_Path.DeterminePreviews()[0x0008f] ;IL_008F
    //
    // This means BuildTool_Path.startObjectId has a positive id that is exceed entity pool
    // May due to local buildTool affect by other player's build request
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.DeterminePreviews))]
    public static Exception DeterminePreviews(Exception __exception, BuildTool_Path __instance)
    {
        if (__exception != null)
        {
            // Reset state
            __instance.startObjectId = 0;
            __instance.startNearestAddonAreaIdx = 0;
            __instance.startTarget = Vector3.zero;
            __instance.pathPointCount = 0;
        }
        return null;
    }

    // IndexOutOfRangeException: Index was outside the bounds of the array.
    // at CargoTraffic.SetBeltState(System.Int32 beltId, System.Int32 state); (IL_002D)
    // at CargoTraffic.SetBeltSelected(System.Int32 beltId); (IL_0000)
    // at PlayerAction_Inspect.GameTick(System.Int64 timei); (IL_053E)
    // 
    // Worst outcome when suppressed: Belt highlight is incorrect
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(CargoTraffic), nameof(CargoTraffic.SetBeltState))]
    public static Exception SetBeltState()
    {
        return null;
    }

    // NullReferenceException: Object reference not set to an instance of an object
    // at BGMController.UpdateLogic();(IL_03BC)
    // at BGMController.LateUpdate(); (IL_0000)
    //
    // This means if (DSPGame.Game.running) is null
    // Worst outcome when suppressed: BGM stops
    [HarmonyFinalizer]
    [HarmonyPatch(typeof(BGMController), nameof(BGMController.UpdateLogic))]
    public static Exception UpdateLogic()
    {
        return null;
    }
}
