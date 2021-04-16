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
                StationUI packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            else if (SimulatedWorld.Initialized)
            {
                return true;
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
                StationUI packet = new StationUI(__instance.station.gid, __instance.stationWindow.factory.planet.id, __instance.index, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            else if (SimulatedWorld.Initialized)
            {
                return true;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnOpen")]
        public static void _OnOpen_Postfix(UIStationStorage __instance)
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new StationSubscribeUIUpdates(true, __instance.station.gid));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("_OnClose")]
        public static void _OnClose_Postfix(UIStationStorage __instance)
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient)
            {
                LocalPlayer.SendPacket(new StationSubscribeUIUpdates(false, __instance.station.gid));
            }
        }
    }
}
