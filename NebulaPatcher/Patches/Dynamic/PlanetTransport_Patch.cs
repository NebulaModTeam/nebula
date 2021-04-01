using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetTransport))]
    class PlanetTransport_Patch
    {
        /*
         * dont patch NewStationComponent because:
         * - it will only get called when clients have the FactoryData loaded as EntityManger.cs does only place buildings if that is the case
         * - it will create a working and functional ILS/PLS/Collector which is not guarantee when doing it myself
         * - it will update the GalacticTransport.stationPool correctly over AddStationComponent()
         * => if we need to handle IL (interPLogistic) for factories we have not loaded we will create a fake entry in GalacticTransport.stationPool
         * which contains only the needed information to make it work. NOTE: this entry needs to get updated over AddStationComponent() once the FactoryData gets loaded
         * NOTE: the most painfull thing will probably be to get the IDs right so the IL actually matches with the host.
         */

        [HarmonyPostfix]
        [HarmonyPatch("SetStationStorage")]
        public static void SetStationStorage_Postfix(PlanetTransport __instance, int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, Player player)
        {
            if (SimulatedWorld.Initialized && !LocalPlayer.PatchLocks["PlanetTransport"])
            {
                StationUI packet = new StationUI(stationId, __instance.planet.id, storageIdx, itemId, itemCountMax, localLogic, remoteLogic);
                LocalPlayer.SendPacket(packet);
            }
        }
    }
}
