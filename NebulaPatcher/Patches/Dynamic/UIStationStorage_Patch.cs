#region

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
// ReSharper disable RedundantAssignment

#endregion

#pragma warning disable Harmony003
namespace NebulaPatcher.Patches.Dynamic;

[HarmonyPatch(typeof(UIStationStorage))]
internal class UIStationStorage_Patch
{
    private static bool eventLock;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationStorage.OnMaxSliderValueChange))]
    public static bool OnMaxSliderValueChangePrefix(UIStationStorage __instance, float val)
    {
        if (!Multiplayer.IsActive || eventLock)
        {
            return !Multiplayer.IsActive;
        }
        if (Math.Abs(val - __instance.station.storage[__instance.index].max / 100f) > 0.000000001)
        {
            // If the slider value doesn't match with storage.max, mark it
            Multiplayer.Session.StationsUI.StorageMaxChangeId = __instance.index;
        }
        return !Multiplayer.IsActive;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationStorage._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Prefix(UIStationStorage __instance, ref float __state)
    {
        // Set up eventLock so value changes in maxSlider.value don't trigger changed check
        eventLock = true;
        __state = __instance.maxSlider.value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationStorage._OnUpdate))]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
    public static void _OnUpdate_Postfix(UIStationStorage __instance, float __state)
    {
        // Restore the silder value so it is not modified by RefreshValues()
        if (Multiplayer.IsActive && Multiplayer.Session.StationsUI.StorageMaxChangeId != -1)
        {
            __instance.maxSlider.value = __state;
            __instance.maxValueText.text = ((int)(__instance.maxSlider.value * 100)).ToString();
        }
        eventLock = false;
    }

    /*
     * host behaves normally and sends update to clients which then apply the changes
     * clients send a request to the server and only run the original method once they receive the response
     */
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationStorage.OnItemIconMouseDown))]
    [HarmonyPriority(Priority.First)]
    public static void OnItemIconMouseDown_Prefix(UIStationStorage __instance, ref (int, int) __state)
    {
        __state = (__instance.station.storage[__instance.index].count, __instance.station.storage[__instance.index].inc);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationStorage.OnItemIconMouseDown))]
    [HarmonyPriority(Priority.Last)]
    public static void OnItemIconMouseDown_Postfix(UIStationStorage __instance, (int, int) __state)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }
        var stationStore = __instance.station.storage[__instance.index];
        if (__state.Item1 == stationStore.count && __state.Item2 == stationStore.inc)
        {
            return;
        }
        var packet = new StorageUI(__instance.stationWindow.factory.planet.id, __instance.station.id,
            __instance.station.gid, __instance.index, stationStore.count, stationStore.inc);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    /*
     * host behaves normally and sends update to clients which then apply the changes
     * clients send a request to the server and only run the original method once they receive the response
     */
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UIStationStorage.OnItemIconMouseUp))]
    [HarmonyPriority(Priority.First)]
    public static void OnItemIconMouseUp_Prefix(UIStationStorage __instance, ref (int, int) __state)
    {
        __state = (__instance.station.storage[__instance.index].count, __instance.station.storage[__instance.index].inc);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationStorage.OnItemIconMouseUp))]
    [HarmonyPriority(Priority.Last)]
    public static void OnItemIconMouseUp_Postfix(UIStationStorage __instance, (int, int) __state)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }
        var stationStore = __instance.station.storage[__instance.index];

        if (__state.Item1 == stationStore.count && __state.Item2 == stationStore.inc)
        {
            return;
        }
        var packet = new StorageUI(__instance.stationWindow.factory.planet.id, __instance.station.id,
            __instance.station.gid, __instance.index, stationStore.count, stationStore.inc);
        Multiplayer.Session.Network.SendPacket(packet);
    }

    /*
     * sync sandbox mode lock station storage function
     */
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UIStationStorage.OnKeepModeButtonClick))]
    public static void OnKeepModeButtonClick_Postfix(UIStationStorage __instance)
    {
        if (!Multiplayer.IsActive || Multiplayer.Session.Ships.PatchLockILS)
        {
            return;
        }
        var stationStore = __instance.station.storage[__instance.index];

        var packet = new StorageUI(__instance.stationWindow.factory.planet.id,
            __instance.station.id, __instance.station.gid, __instance.index, (byte)stationStore.keepMode);
        Multiplayer.Session.Network.SendPacket(packet);
    }
}
#pragma warning restore Harmony003
