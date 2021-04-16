using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(UIStationWindow))]
    class UIStationWindow_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("OnMaxChargePowerSliderValueChange")]
        public static bool OnMaxChargePowerSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 0, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMaxTripDroneSliderValueChange")]
        public static bool OnMaxTripDroneSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 1, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMaxTripVesselSliderValueChange")]
        public static bool OnMaxTripVesselSliderValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 2, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMinDeliverDroneValueChange")]
        public static bool OnMinDeliverDroneValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 3, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnMinDeliverVesselValueChange")]
        public static bool OnMinDeliverVesselValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 4, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperDistanceValueChange")]
        public static bool OnWarperDistanceValueChange_Postfix(UIStationWindow __instance, float value)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 5, value);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperNecessaryClick")]
        public static bool OnWarperNecessaryClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 6, 0f);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnIncludeOrbitCollectorClick")]
        public static bool OnIncludeOrbitCollectorClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 7, 0f);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnDroneIconClick")]
        public static bool OnDroneIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 8, __instance.transport.stationPool[__instance.stationId].idleDroneCount);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnShipIconClick")]
        public static bool OnShipIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 9, __instance.transport.stationPool[__instance.stationId].idleShipCount);
                LocalPlayer.SendPacket(packet);
                if (LocalPlayer.IsMasterClient)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnWarperIconClick")]
        public static bool OnWarperIconClick_Postfix(UIStationWindow __instance, int obj)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["UIStationWindow"])
            {
                StationUI packet = new StationUI(__instance.factory.transport.stationPool[__instance.stationId].gid, __instance.factory.planet.id, 10, __instance.transport.stationPool[__instance.stationId].warperCount);
                LocalPlayer.SendPacket(packet);
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
