using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationWindow))]
    class UIStationWindow_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnMaxChargePowerSliderValueChange")]
        public static void OnMaxChargePowerSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 0, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMaxTripDroneSliderValueChange")]
        public static void OnMaxTripDroneSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 1, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMaxTripVesselSliderValueChange")]
        public static void OnMaxTripVesselSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 2, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMinDeliverDroneValueChange")]
        public static void OnMinDeliverDroneValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 3, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMinDeliverVesselValueChange")]
        public static void OnMinDeliverVesselValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 4, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnWarperDistanceValueChange")]
        public static void OnWarperDistanceValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (!LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 5, value);
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
