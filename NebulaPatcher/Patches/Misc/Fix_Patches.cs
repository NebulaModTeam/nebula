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
    // at BuildTool.UpdateGizmos (BuildModel model) [0x0009a] ;IL_009A
    // at BuildTool_Path.UpdateGizmos (BuildModel model) [0x00000] ;IL_0000
    // at BuildTool_Path._OnTick (long time) [0x00051] ;IL_0051
    // at BuildTool._GameTick (long time) [0x000cf] ;IL_00CF
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildTool), nameof(BuildTool.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.UpdateGizmos))]
    public static void UpdateGizmos_Prefix(BuildTool __instance, BuildModel model)
    {
        var factory = __instance?.factory;
        if (factory == null)
        {
            return;
        }

        if (model != null)
        {
            if (model.startGizmoObjId > 0 && (factory.entityPool == null || model.startGizmoObjId >= factory.entityPool.Length))
            {
                model.startGizmoObjId = 0;
            }
            else if (model.startGizmoObjId < 0 && (factory.prebuildPool == null || -model.startGizmoObjId >= factory.prebuildPool.Length))
            {
                model.startGizmoObjId = 0;
            }

            if (model.endGizmoObjId > 0 && (factory.entityPool == null || model.endGizmoObjId >= factory.entityPool.Length))
            {
                model.endGizmoObjId = 0;
            }
            else if (model.endGizmoObjId < 0 && (factory.prebuildPool == null || -model.endGizmoObjId >= factory.prebuildPool.Length))
            {
                model.endGizmoObjId = 0;
            }
        }

        if (__instance is BuildTool_Path pathTool)
        {
            if (pathTool.startObjectId > 0 && (factory.entityPool == null || pathTool.startObjectId >= factory.entityPool.Length))
            {
                pathTool.startObjectId = 0;
            }
            else if (pathTool.startObjectId < 0 && (factory.prebuildPool == null || -pathTool.startObjectId >= factory.prebuildPool.Length))
            {
                pathTool.startObjectId = 0;
            }

            if (pathTool.castObjectId > 0 && (factory.entityPool == null || pathTool.castObjectId >= factory.entityPool.Length))
            {
                pathTool.castObjectId = 0;
            }
            else if (pathTool.castObjectId < 0 && (factory.prebuildPool == null || -pathTool.castObjectId >= factory.prebuildPool.Length))
            {
                pathTool.castObjectId = 0;
            }
        }
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(BuildTool), nameof(BuildTool.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Path), nameof(BuildTool_Path.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Addon), nameof(BuildTool_Addon.UpdateGizmos))]
    [HarmonyPatch(typeof(BuildTool_Inserter), nameof(BuildTool_Inserter.UpdateGizmos))]
    public static Exception UpdateGizmos_Finalizer(Exception __exception, BuildTool __instance, BuildModel model)
    {
        if (__exception != null)
        {
            if (model != null)
            {
                model.startGizmoObjId = 0;
                model.endGizmoObjId = 0;
                model.previewGizmoOn = false;
            }
            if (__instance is BuildTool_Path pathTool)
            {
                pathTool.startObjectId = 0;
                pathTool.castObjectId = 0;
                pathTool.startNearestAddonAreaIdx = 0;
                pathTool.startTarget = Vector3.zero;
                pathTool.pathPointCount = 0;
            }
        }
        return null;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BuildTool), nameof(BuildTool.GetPrefabDesc))]
    public static bool GetPrefabDesc_Prefix(BuildTool __instance, int objId, ref PrefabDesc __result)
    {
        var factory = __instance?.factory;
        if (factory == null || objId == 0)
        {
            __result = null;
            return false;
        }

        if (objId > 0)
        {
            if (factory.entityPool == null || objId >= factory.entityPool.Length)
            {
                __result = null;
                return false;
            }
        }
        else
        {
            var prebuildId = -objId;
            if (factory.prebuildPool == null || prebuildId >= factory.prebuildPool.Length)
            {
                __result = null;
                return false;
            }
        }

        return true;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(BuildTool), nameof(BuildTool.GetPrefabDesc))]
    public static Exception GetPrefabDesc_Finalizer(Exception __exception, ref PrefabDesc __result)
    {
        if (__exception != null)
        {
            __result = null;
            return null;
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
