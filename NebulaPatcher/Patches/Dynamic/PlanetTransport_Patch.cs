using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PlanetTransport))]
    class PlanetTransport_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SetStationStorage")]
        public static void SetStationStorage_Postfix(int stationId, int storageIdx, int itemId, int itemCountMax, ELogisticStorage localLogic, ELogisticStorage remoteLogic, StorageComponent package)
        {
            StationUI packet = new StationUI(stationId, storageIdx, itemId, itemCountMax, localLogic, remoteLogic);
            LocalPlayer.SendPacket(packet);
        }
    }
}
