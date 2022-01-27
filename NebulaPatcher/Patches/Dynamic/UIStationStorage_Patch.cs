using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine.EventSystems;


namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationStorage))]
    internal class UIStationStorage_Patch
    {
        private static bool eventLock;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationStorage.OnMaxSliderValueChange))]
        public static bool OnMaxSliderValueChangePrefix(UIStationStorage __instance, float val)
        {
            if (Multiplayer.IsActive && !eventLock)
            {
                if (val != (float)(__instance.station.storage[__instance.index].max / 100))
                {
                    // If the silder value doesn't match with storage.max, mark it
                    Multiplayer.Session.StationsUI.StorageMaxChangeId = __instance.index;
                }
            }
            return !Multiplayer.IsActive;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationStorage._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
        public static void _OnUpdate_Prefix(UIStationStorage __instance, ref float __state)
        {
            // Set up eventLock so value changes in maxSlider.value don't trigger changed check
            eventLock = true;
            __state = __instance.maxSlider.value;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UIStationStorage._OnUpdate))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Original Function Name")]
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
        public static bool OnItemIconMouseDown_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                Multiplayer.Session.StationsUI.LastMouseEvent = evt;
                Multiplayer.Session.StationsUI.LastMouseEventWasDown = true;
                StationUI packet;
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    PointerEventData pointEventData = evt as PointerEventData;
                    if (GameMain.mainPlayer.inhandItemId == __instance.station.storage[__instance.index].itemId && pointEventData.button == PointerEventData.InputButton.Left)
                    {
                        int diff = __instance.station.storage[__instance.index].max - __instance.station.storage[__instance.index].count;
                        int amount = (diff >= GameMain.mainPlayer.inhandItemCount) ? GameMain.mainPlayer.inhandItemCount : diff;
                        if (amount < 0)
                        {
                            amount = 0;
                        }
                        packet = new StationUI(__instance.stationWindow.factory.planet.id, __instance.station.id, __instance.station.gid, __instance.index, StationUI.EUISettings.AddOrRemoveItemFromStorageResponse, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count + amount);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                }
                else
                {
                    packet = new StationUI(__instance.stationWindow.factory.planet.id, __instance.station.id, __instance.station.gid, __instance.index, StationUI.EUISettings.AddOrRemoveItemFromStorageRequest, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                    Multiplayer.Session.Network.SendPacket(packet);
                }
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /*
         * host behaves normally and sends update to clients which then apply the changes
         * clients send a request to the server and only run the original method once they receive the response
         */
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UIStationStorage.OnItemIconMouseUp))]
        public static bool OnItemIconMouseUp_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (Multiplayer.IsActive && !Multiplayer.Session.Ships.PatchLockILS)
            {
                Multiplayer.Session.StationsUI.LastMouseEvent = evt;
                Multiplayer.Session.StationsUI.LastMouseEventWasDown = false;
                StationUI packet;
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    if (__instance.insplit)
                    {
                        int splitVal = UIRoot.instance.uiGame.gridSplit.value;
                        int diff = (splitVal >= __instance.station.storage[__instance.index].count) ? __instance.station.storage[__instance.index].count : splitVal;
                        packet = new StationUI(__instance.stationWindow.factory.planet.id, __instance.station.id, __instance.station.gid, __instance.index, StationUI.EUISettings.AddOrRemoveItemFromStorageResponse, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count - diff);
                        Multiplayer.Session.Network.SendPacket(packet);
                    }
                }
                else
                {
                    packet = new StationUI(__instance.stationWindow.factory.planet.id, __instance.station.id, __instance.station.gid, __instance.index, StationUI.EUISettings.AddOrRemoveItemFromStorageRequest, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                    Multiplayer.Session.Network.SendPacket(packet);
                }
                if (Multiplayer.Session.LocalPlayer.IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
