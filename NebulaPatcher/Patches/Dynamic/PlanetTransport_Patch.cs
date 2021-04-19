using HarmonyLib;
using NebulaModel.Packets.Logistics;
using NebulaWorld;
using UnityEngine;

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
                foreach(StationComponent stationComponent in GameMain.data.galacticTransport.stationPool)
                {
                    if(stationComponent != null && stationComponent.planetId == GameMain.data.localPlanet.id && stationComponent.id == stationId)
                    {
                        StationUI packet = new StationUI(stationComponent.gid, __instance.planet.id, storageIdx, itemId, itemCountMax, localLogic, remoteLogic);
                        LocalPlayer.SendPacket(packet);
                        break;
                    }
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
