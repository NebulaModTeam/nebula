using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(BuildTool_Inserter))]
    class BuildTool_Inserter_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreatePrebuilds")]
        public static bool CreatePrebuilds_Prefix(BuildTool_Inserter __instance)
        {
            return BuildToolManager.CreatePrebuilds(__instance);
        }
    }
}
