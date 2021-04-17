using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;
using UnityEngine.EventSystems;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationStorage))]
    class UIStationStorage_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnItemIconMouseDown")]
        public static bool OnItemIconMouseDown_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationStorage"])
            {
                StationUIManager.lastMouseEvent = evt;
                StationUIManager.lastMouseEventWasDown = true;
                StationUI packet;
                if (LocalPlayer.IsMasterClient)
                {
                    PointerEventData pointEventData = evt as PointerEventData;
                    if(GameMain.mainPlayer.inhandItemId == __instance.station.storage[__instance.index].itemId && pointEventData.button == PointerEventData.InputButton.Left)
                    {
                        int diff = __instance.station.storage[__instance.index].max - __instance.station.storage[__instance.index].count;
                        int amount = (diff >= GameMain.mainPlayer.inhandItemCount) ? GameMain.mainPlayer.inhandItemCount : diff;
                        if (amount < 0)
                        {
                            amount = 0;
                        }
                        packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 12, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count + amount);
                        LocalPlayer.SendPacket(packet);
                    }
                }
                else
                {
                    packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                    LocalPlayer.SendPacket(packet);
                }
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnItemIconMouseUp")]
        public static bool OnItemIconMouseUp_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationStorage"])
            {
                StationUIManager.lastMouseEvent = evt;
                StationUIManager.lastMouseEventWasDown = false;
                StationUI packet;
                if (LocalPlayer.IsMasterClient)
                {
                    if ((bool)AccessTools.Field(typeof(UIStationStorage), "insplit").GetValue(__instance))
                    {
                        int splitVal = UIRoot.instance.uiGame.gridSplit.value;
                        int diff = (splitVal >= __instance.station.storage[__instance.index].count) ? __instance.station.storage[__instance.index].count : splitVal;
                        packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 12, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count - diff);
                        LocalPlayer.SendPacket(packet);
                    }
                }
                else
                {
                    packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                    LocalPlayer.SendPacket(packet);
                }
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
