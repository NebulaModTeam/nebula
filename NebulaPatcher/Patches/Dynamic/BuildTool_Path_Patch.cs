using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(BuildTool_Path))]
    class BuildTool_Path_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreatePrebuilds")]
        public static bool CreatePrebuilds_Prefix(BuildTool_Path __instance)
        {
            return BuildToolManager.CreatePrebuilds(__instance);
        }
    }
}
