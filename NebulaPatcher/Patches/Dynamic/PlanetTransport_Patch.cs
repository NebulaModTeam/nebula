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

        // for the PLS slot to sync properly the StationComponent of the PLS needs to have planetId set to the correct value.
        // as the game does not do that for some reason, we need to do it here
        [HarmonyPostfix]
        [HarmonyPatch("NewStationComponent")]
        public static void NewStationComponent_Postfix(PlanetTransport __instance, StationComponent __result, int _entityId, int _pcId, PrefabDesc _desc)
        {
            if (!__result.isStellar && __result.planetId == 0)
            {
                __result.planetId = __instance.planet.id;
            }
        }
    }
}
