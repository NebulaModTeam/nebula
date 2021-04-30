using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using NebulaWorld.Logistics;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetTransport))]
    class PlanetTransport_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetStationStorage")]
        public static bool SetStationStorage_Postfix(PlanetTransport __instance, int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, Player player)
        {
            if (SimulatedWorld.Initialized && !ILSShipManager.PatchLockILS)
            {
                StationComponent stationComponent = __instance.stationPool[stationId];
                if(stationComponent != null)
                {
                    int id = ((stationComponent.isStellar == true) ? stationComponent.gid : stationComponent.id);
                    StationUI packet = new StationUI(id, __instance.planet.id, storageIdx, itemId, itemCountMax, localLogic, remoteLogic, stationComponent.isStellar);
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

        /*
         * As clients need to access the StationComponent in gStationPool when RematchRemotePairs() is called (and this also gets called by RemoveStationComponent())
         * we need to prevent the call for client here to avoid a NRE and instead call it triggered by host after RematchRemotePairs() got called.
         * basically in a Postfix of RemoveStationComponent()
         * Clients do not call RematchRemotePairs() (as we block it), but update ships when host calls it via ILSShipDataUpdate packet
         */
        [HarmonyPrefix]
        [HarmonyPatch("RemoveStationComponent")]
        public static bool RemoveStationComponent_Prefix(PlanetTransport __instance, int id)
        {
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient || ILSShipManager.PatchLockILS;
        }

        /*
         * Host has called RematchRemotePairs() now and thus has send the ILSShipDataUpdate packet, so we can savely tell clients to remove the station component now.
         */
        [HarmonyPostfix]
        [HarmonyPatch("RemoveStationComponent")]
        public static void RemoveStationComponent_Postfix(PlanetTransport __instance, int id)
        {
            if (!SimulatedWorld.Initialized || !LocalPlayer.IsMasterClient)
            {
                return;
            }
            LocalPlayer.SendPacket(new ILSRemoveStationComponent(id, __instance.planet.id, __instance.stationPool[id].gid));
        }
    }
}
