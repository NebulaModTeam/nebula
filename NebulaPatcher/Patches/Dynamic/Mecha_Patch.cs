using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(Mecha))]
    class Mecha_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GameTick")]
        public static void GameTick_Postfix(long time, float dt)
        {
            if (SimulatedWorld.Initialized)
            {
                SimulatedWorld.OnDronesGameTick(time, dt);
            }
        }

        // We can't do this as client as we won't be able to get_nearestPlanet() since we do not currently have all of the factory info
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddConsumptionStat))]
        [HarmonyPatch(typeof(Mecha), nameof(Mecha.AddProductionStat))]
        public static bool AddStat_Common_Prefix(ref PlanetFactory factory)
        {
            if(!SimulatedWorld.Initialized || LocalPlayer.IsMasterClient)
                return true;

            // TODO: Send packet to host to add stat?
            // Easy option: just have host add stat to their closest planet, though is this better than not syncing it at all?
            // Hard option: find a way to reliably get nearest planet from client with missing data

            return false;
        }
    }
}
