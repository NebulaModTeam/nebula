using HarmonyLib;
using NebulaWorld;
using NebulaModel.Logger;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport))]
    class GalacticTransport_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("AddStationComponent")]
        public static bool AddStationComponent_Prefix(GalacticTransport __instance, int planetId, StationComponent station)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            // we use the GameMain.data.galacticTransport.stationPool array in our own way, so prevent the client from using it with vanilla code
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("RemoveStationComponent")]
        public static bool RemoveStationComponent_Prefix(GalacticTransport __instance, int gid)
        {
            if (!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
            {
                return true;
            }
            // we use the GameMain.data.galacticTransport.stationPool array in our own way, so prevent the client from using it with vanilla code
            return false;
        }
    }
}
