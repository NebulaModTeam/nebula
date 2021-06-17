using HarmonyLib;
using NebulaWorld.Factory;

namespace NebulaPatcher.Patches.Dynamic
{
    [HarmonyPatch(typeof(BuildTool_Click))]
    class BuildTool_Click_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreatePrebuilds")]
        public static bool CreatePrebuilds_Prefix(BuildTool_Click __instance)
        {
            return BuildToolManager.CreatePrebuilds(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch("CheckBuildConditions")]
        public static bool CheckBuildConditions(ref bool __result)
        {
            if (FactoryManager.IgnoreBasicBuildConditionChecks)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
