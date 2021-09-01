using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(GuideMissionStandardMode))]
    class GuideMissionStandardMode_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GuideMissionStandardMode.Skip))]
        public static bool Skip_Prefix()
        {
            //This prevents spawning landing capsule and preparing spawn area for the clients in multiplayer.
            return !Multiplayer.IsActive || ((LocalPlayer)Multiplayer.Session.LocalPlayer).IsHost;
        }
    }
}
