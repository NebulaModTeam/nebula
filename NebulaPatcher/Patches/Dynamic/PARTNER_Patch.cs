using HarmonyLib;
using NebulaWorld;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(PARTNER))]
    internal class PARTNER_Patch
    {
        //[HarmonyPrefix]
        //[HarmonyPatch(nameof(PARTNER.UploadClusterGenerationToGalaxyServer))]
        public static bool UploadClusterGenerationToGalaxyServer_Prefix()
        {
            // We don't want to upload Milky Way data if we are playing MP
            return !Multiplayer.IsActive;
        }
    }
}
