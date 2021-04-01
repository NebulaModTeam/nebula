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
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 0, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMaxTripDroneSliderValueChange")]
        public static void OnMaxTripDroneSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 1, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMaxTripVesselSliderValueChange")]
        public static void OnMaxTripVesselSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 2, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMinDeliverDroneValueChange")]
        public static void OnMinDeliverDroneValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 3, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnMinDeliverVesselValueChange")]
        public static void OnMinDeliverVesselValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 4, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnWarperDistanceValueChange")]
        public static void OnWarperDistanceValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 5, value);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnWarperNecessaryClick")]
        public static void OnWarperNecessaryClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 6, 0f);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnIncludeOrbitCollectorClick")]
        public static void OnIncludeOrbitCollectorClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 7, 0f);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDroneIconClick")]
        public static void OnDroneIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 8, __instance.transport.stationPool[__instance.stationId].idleDroneCount);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnShipIconClick")]
        public static void OnShipIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 9, __instance.transport.stationPool[__instance.stationId].idleShipCount);
                LocalPlayer.SendPacket(packet);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnWarperIconClick")]
        public static void OnWarperIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.stationId, __instance.factory.planet.id, 10, __instance.transport.stationPool[__instance.stationId].warperCount);
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
