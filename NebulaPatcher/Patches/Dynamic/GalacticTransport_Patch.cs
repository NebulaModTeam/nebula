using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GalacticTransport), "AddStationComponent")]
    class GalacticTransport_Patch
    {
        public static void Postfix(int planetId, StationComponent station)
        {
            foreach(int i in LocalPlayer.PlanetIdsWithLogistics)
            {
                if(i == planetId)
                {
                    return;
                }
            }
            LocalPlayer.PlanetIdsWithLogistics.Add(planetId);
        }
    }
}
