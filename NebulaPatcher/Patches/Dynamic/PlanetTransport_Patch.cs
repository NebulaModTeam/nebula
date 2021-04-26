using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetTransport))]
    class PlanetTransport_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetStationStorage")]
        public static bool SetStationStorage_Postfix(PlanetTransport __instance, int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, Player player)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["PlanetTransport"])
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
    }
}
