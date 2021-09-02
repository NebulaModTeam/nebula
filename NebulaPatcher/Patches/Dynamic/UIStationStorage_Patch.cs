using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine.EventSystems;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationStorage))]
    class UIStationStorage_Patch
    {
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
                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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
                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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
                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
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
                if (((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
