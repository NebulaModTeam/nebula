using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GuideMissionStandardMode), "Skip")]
    class GuideMissionStandardMode_Patch
    {
        public static bool Prefix()
        {
            //This prevents spawning landing capsule and preparing spawn area for the clients in multiplayer.
            return !SimulatedWorld.Initialized || LocalPlayer.IsMasterClient;
        }
    }
}
