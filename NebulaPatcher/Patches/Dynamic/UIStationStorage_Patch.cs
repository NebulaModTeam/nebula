using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine.EventSystems;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationStorage))]
    class UIStationStorage_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnItemIconMouseDown")]
        public static void OnItemIconMouseDown_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (SimulatedWorld.Initialized)
            {
                StationUI packet = new StationUI(__instance.station.id, __instance.stationWindow.factory.planet.id, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnItemIconMouseUp")]
        public static void OnItemIconMouseUp_Postfix(UIStationStorage __instance, BaseEventData evt)
        {
            if (SimulatedWorld.Initialized)
            {
                StationUI packet = new StationUI(__instance.station.id, __instance.stationWindow.factory.planet.id, 11, __instance.station.storage[__instance.index].itemId, __instance.station.storage[__instance.index].count);
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
