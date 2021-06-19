using HarmonyLib;
using NebulaWorld;
using NebulaWorld.Factory;
using UnityEngine;

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

        [HarmonyPostfix]
        [HarmonyPatch("CreatePrebuilds")]
        public static void CreatePrebuilds_Postfix()
        {
            if(SimulatedWorld.Initialized && !LocalPlayer.IsMasterClient && FactoryManager.EventFromServer && FactoryManager.IsHumanInput)
            {
                FactoryManager.IsHumanInput = false;
            }
            else if(SimulatedWorld.Initialized && LocalPlayer.IsMasterClient && (FactoryManager.IsHumanInput || FactoryManager.IsFromClient))
            {
                FactoryManager.IsFromClient = false;
                FactoryManager.IsHumanInput = false;
            }
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
